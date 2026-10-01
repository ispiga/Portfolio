using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Portfolio.Application.Certifications;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Storage;
using Portfolio.Web.Authentication;
using Xunit;

namespace Portfolio.Tests;

public sealed class CertificationMediaStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly Guid certificationId = Guid.NewGuid();
    private readonly TestContextFactory factory;
    private readonly CertificationMediaStorageService service;

    public CertificationMediaStorageTests()
    {
        factory = new TestContextFactory(Guid.NewGuid().ToString());
        using (var context = factory.CreateDbContext())
        {
            context.Certifications.Add(new Certification
            {
                Id = certificationId,
                Translations = [new CertificationTranslation
                {
                    LanguageCode = "es-ES",
                    Name = "Certificación de prueba",
                    Issuer = "Emisor de prueba"
                }]
            });
            context.SaveChanges();
        }

        var environment = new TestWebHostEnvironment(root);
        service = new CertificationMediaStorageService(
            factory,
            environment,
            Options.Create(new CertificationMediaStorageOptions
            {
                Directory = Path.Combine(root, "private-files"),
                MaximumFileSizeBytes = 32
            }));
    }

    [Fact]
    public async Task Attachments_are_stored_outside_web_root_and_publicly_readable()
    {
        var payload = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\ncredential");
        var uploaded = await service.UploadAttachmentAsync(
            certificationId,
            "credential.pdf",
            "application/pdf",
            payload.Length,
            new MemoryStream(payload));

        Assert.True(uploaded.Succeeded);
        Assert.NotNull(uploaded.Attachment);
        var imagePayload = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var imageUpload = await service.UploadAttachmentAsync(
            certificationId,
            "proof.png",
            "image/png",
            imagePayload.Length,
            new MemoryStream(imagePayload));
        Assert.True(imageUpload.Succeeded);

        var attachments = await service.GetAttachmentsAsync(certificationId);
        Assert.Equal(2, attachments.Count);
        var attachment = attachments.Single(item => item.Id == uploaded.Attachment.Id);
        Assert.Equal("credential.pdf", attachment.OriginalFileName);
        Assert.Equal("credential.pdf", attachment.DisplayName);
        Assert.Equal(payload.Length, attachment.SizeBytes);

        Assert.True(await service.UpdateAttachmentDisplayNameAsync(attachment.Id, "  Certificado profesional  "));
        var renamedAttachment = (await service.GetAttachmentsAsync(certificationId)).Single(item => item.Id == attachment.Id);
        Assert.Equal("Certificado profesional", renamedAttachment.DisplayName);
        Assert.Equal("credential.pdf", renamedAttachment.OriginalFileName);
        Assert.False(await service.UpdateAttachmentDisplayNameAsync(attachment.Id, " "));
        Assert.False(await service.UpdateAttachmentDisplayNameAsync(attachment.Id, new string('x', 256)));

        var publicContent = await service.OpenAttachmentAsync(attachment.Id);
        Assert.NotNull(publicContent);
        await using (publicContent.Content)
        {
            Assert.Equal(payload, await ReadAllAsync(publicContent.Content));
        }

        await using var context = factory.CreateDbContext();
        var storageKey = await context.CertificationAttachments
            .Where(item => item.Id == attachment.Id)
            .Select(item => item.StorageKey)
            .SingleAsync();
        var storedPath = Path.Combine(root, "private-files", storageKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(storedPath));
        Assert.False(storedPath.StartsWith(Path.Combine(root, "wwwroot"), StringComparison.OrdinalIgnoreCase));

        Assert.True(await service.DeleteAttachmentAsync(attachment.Id));
        Assert.False(File.Exists(storedPath));
        Assert.Null(await service.OpenAttachmentAsync(attachment.Id));
        Assert.Single(await service.GetAttachmentsAsync(certificationId));
    }

    [Fact]
    public async Task Upload_rejects_invalid_signatures_mime_types_and_oversized_files()
    {
        var invalid = await service.UploadAttachmentAsync(
            certificationId,
            "invalid.pdf",
            "application/pdf",
            7,
            new MemoryStream(System.Text.Encoding.ASCII.GetBytes("not pdf")));
        Assert.Equal(CertificationAttachmentError.InvalidContent, invalid.Error);

        var mismatched = await service.UploadAttachmentAsync(
            certificationId,
            "image.png",
            "application/pdf",
            8,
            new MemoryStream(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }));
        Assert.Equal(CertificationAttachmentError.UnsupportedType, mismatched.Error);

        var oversized = await service.UploadAttachmentAsync(
            certificationId,
            "large.pdf",
            "application/pdf",
            33,
            new MemoryStream(new byte[33]));
        Assert.Equal(CertificationAttachmentError.TooLarge, oversized.Error);
        Assert.Empty(await service.GetAttachmentsAsync(certificationId));
    }

    [Fact]
    public async Task Card_image_replacement_updates_the_stored_version_and_removes_the_old_file()
    {
        var firstImage = new byte[] { 0xFF, 0xD8, 0xFF, 0x01 };
        var firstResult = await service.UploadCardImageAsync(
            certificationId,
            "card.jpg",
            "image/jpeg",
            firstImage.Length,
            new MemoryStream(firstImage));
        Assert.True(firstResult.Succeeded);

        await using var firstContext = factory.CreateDbContext();
        var firstKey = await firstContext.Certifications
            .Where(item => item.Id == certificationId)
            .Select(item => item.ImagePath)
            .SingleAsync();
        Assert.NotNull(firstKey);
        var firstPath = Path.Combine(root, "private-files", firstKey.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(firstPath));

        var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        var replacement = await service.UploadCardImageAsync(
            certificationId,
            "card.png",
            "image/png",
            png.Length,
            new MemoryStream(png));
        Assert.True(replacement.Succeeded);
        Assert.False(File.Exists(firstPath));

        var image = await service.OpenCardImageAsync(certificationId);
        Assert.NotNull(image);
        Assert.Equal("image/png", image.ContentType);
        await using (image.Content)
        {
            Assert.Equal(png, await ReadAllAsync(image.Content));
        }

        var publicCertification = Assert.Single(await new Portfolio.Infrastructure.Services.CertificationQueryService(factory).GetCertificationsAsync());
        Assert.StartsWith($"/certification-card-images/{certificationId}?v=", publicCertification.ImagePath);
        Assert.True(await service.DeleteCardImageAsync(certificationId));
        Assert.Null(await service.OpenCardImageAsync(certificationId));
    }

    [Fact]
    public async Task Public_media_endpoints_allow_anonymous_access_and_disable_caching()
    {
        var payload = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\npublic");
        var upload = await service.UploadAttachmentAsync(
            certificationId,
            "public.pdf",
            "application/pdf",
            payload.Length,
            new MemoryStream(payload));
        Assert.True(upload.Succeeded);

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Services.AddSingleton<ICertificationMediaService>(service);
        await using var app = builder.Build();
        app.MapCertificationMediaEndpoints();
        await app.StartAsync();

        using var response = await app.GetTestClient().GetAsync($"/certification-attachments/{upload.Attachment!.Id}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("inline", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal(payload, await response.Content.ReadAsByteArrayAsync());
    }

    [Fact]
    public async Task Deleting_certification_removes_all_media_metadata_and_files()
    {
        var pdf = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\ncredential");
        var attachment = await service.UploadAttachmentAsync(
            certificationId,
            "credential.pdf",
            "application/pdf",
            pdf.Length,
            new MemoryStream(pdf));
        var jpeg = new byte[] { 0xFF, 0xD8, 0xFF, 0x01 };
        var image = await service.UploadCardImageAsync(
            certificationId,
            "card.jpg",
            "image/jpeg",
            jpeg.Length,
            new MemoryStream(jpeg));
        Assert.True(attachment.Succeeded);
        Assert.True(image.Succeeded);

        await using var context = factory.CreateDbContext();
        var attachmentPath = await context.CertificationAttachments
            .Where(item => item.Id == attachment.Attachment!.Id)
            .Select(item => item.StorageKey)
            .SingleAsync();
        var imagePath = await context.Certifications
            .Where(item => item.Id == certificationId)
            .Select(item => item.ImagePath)
            .SingleAsync();
        var attachmentFile = Path.Combine(root, "private-files", attachmentPath.Replace('/', Path.DirectorySeparatorChar));
        var imageFile = Path.Combine(root, "private-files", imagePath!.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(attachmentFile));
        Assert.True(File.Exists(imageFile));

        var result = await new Portfolio.Infrastructure.Services.CertificationAdministrationService(factory, service)
            .DeleteAsync(certificationId);

        Assert.Equal(CertificationDeleteResult.Deleted, result);
        Assert.False(File.Exists(attachmentFile));
        Assert.False(File.Exists(imageFile));
        Assert.Null(await service.OpenAttachmentAsync(attachment.Attachment.Id));
        Assert.Null(await service.OpenCardImageAsync(certificationId));
        Assert.Null(await new Portfolio.Infrastructure.Services.CertificationAdministrationService(factory, service)
            .GetCertificationAsync(certificationId));
    }

    public void Dispose()
    {
        if (Directory.Exists(root))
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private static async Task<byte[]> ReadAllAsync(Stream stream)
    {
        using var memory = new MemoryStream();
        await stream.CopyToAsync(memory);
        return memory.ToArray();
    }

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }

    private sealed class TestWebHostEnvironment(string contentRoot) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "Portfolio.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = Path.Combine(contentRoot, "wwwroot");
        public string EnvironmentName { get; set; } = "Testing";
        public string ContentRootPath { get; set; } = contentRoot;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
