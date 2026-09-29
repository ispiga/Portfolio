using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Portfolio.Application.Experiences;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Storage;
using Xunit;

namespace Portfolio.Tests;

public sealed class ExperienceAttachmentStorageTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
    private readonly Guid experienceId = Guid.NewGuid();
    private readonly TestContextFactory factory;
    private readonly ExperienceAttachmentStorageService service;

    public ExperienceAttachmentStorageTests()
    {
        factory = new TestContextFactory(Guid.NewGuid().ToString());
        using (var context = factory.CreateDbContext())
        {
            context.Experiences.Add(new Experience
            {
                Id = experienceId,
                StartDate = new DateOnly(2020, 1, 1)
            });
            context.SaveChanges();
        }

        var environment = new TestWebHostEnvironment(root);
        var options = Options.Create(new ExperienceAttachmentStorageOptions
        {
            Directory = Path.Combine(root, "private-files"),
            MaximumFileCount = 1,
            MaximumFileSizeBytes = 1024
        });
        service = new ExperienceAttachmentStorageService(
            factory,
            environment,
            options,
            NullLogger<ExperienceAttachmentStorageService>.Instance);
    }

    [Fact]
    public async Task Valid_pdf_is_stored_privately_and_count_limit_is_enforced()
    {
        var payload = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\nprivate attachment");
        var first = await service.UploadAsync(
            experienceId,
            "recommendation.pdf",
            "application/pdf",
            payload.Length,
            new MemoryStream(payload));

        Assert.True(first.Succeeded);
        Assert.NotNull(first.Attachment);
        Assert.False(first.Attachment.IsPublic);
        var saved = Assert.Single(await service.GetAttachmentsAsync(experienceId));
        Assert.Equal("recommendation.pdf", saved.OriginalFileName);
        Assert.Equal("recommendation.pdf", saved.DisplayName);
        Assert.False(saved.IsPublic);
        Assert.Equal(payload.Length, saved.SizeBytes);
        Assert.Null(await service.OpenPublicReadAsync(saved.Id));

        Assert.True(await service.SetPublicAsync(saved.Id, true));
        var published = await service.OpenPublicReadAsync(saved.Id);
        Assert.NotNull(published);
        await published.Content.DisposeAsync();

        Assert.True(await service.SetPublicAsync(saved.Id, false));
        Assert.Null(await service.OpenPublicReadAsync(saved.Id));

        var second = await service.UploadAsync(
            experienceId,
            "second.pdf",
            "application/pdf",
            payload.Length,
            new MemoryStream(payload));
        Assert.Equal(ExperienceAttachmentError.TooManyFiles, second.Error);

        var download = await service.OpenReadAsync(saved.Id);
        Assert.NotNull(download);
        await using (download.Content)
        {
            Assert.Equal(payload, await ReadAllAsync(download.Content));
        }
    }

    [Fact]
    public async Task Display_name_can_be_changed_without_changing_the_original_file_name()
    {
        var payload = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\nprivate attachment");
        var result = await service.UploadAsync(
            experienceId,
            "recommendation.pdf",
            "application/pdf",
            payload.Length,
            new MemoryStream(payload));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Attachment);

        Assert.True(await service.UpdateDisplayNameAsync(result.Attachment.Id, "  Carta de recomendación  "));

        var saved = Assert.Single(await service.GetAttachmentsAsync(experienceId));
        Assert.Equal("recommendation.pdf", saved.OriginalFileName);
        Assert.Equal("Carta de recomendación", saved.DisplayName);
    }

    [Fact]
    public async Task Display_name_update_rejects_blank_or_too_long_names()
    {
        var payload = System.Text.Encoding.ASCII.GetBytes("%PDF-1.7\nprivate attachment");
        var result = await service.UploadAsync(
            experienceId,
            "recommendation.pdf",
            "application/pdf",
            payload.Length,
            new MemoryStream(payload));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Attachment);

        Assert.False(await service.UpdateDisplayNameAsync(result.Attachment.Id, "   "));
        Assert.False(await service.UpdateDisplayNameAsync(result.Attachment.Id, new string('x', 256)));
    }

    [Fact]
    public async Task Upload_rejects_mismatched_signatures_and_oversized_files()
    {
        var invalidPdf = System.Text.Encoding.ASCII.GetBytes("not a pdf");
        var invalid = await service.UploadAsync(
            experienceId,
            "fake.pdf",
            "application/pdf",
            invalidPdf.Length,
            new MemoryStream(invalidPdf));
        Assert.Equal(ExperienceAttachmentError.InvalidContent, invalid.Error);

        var oversized = await service.UploadAsync(
            experienceId,
            "large.pdf",
            "application/pdf",
            1025,
            new MemoryStream(new byte[1025]));
        Assert.Equal(ExperienceAttachmentError.TooLarge, oversized.Error);
        Assert.Empty(await service.GetAttachmentsAsync(experienceId));
    }

    [Fact]
    public async Task Unsupported_extensions_and_mime_types_are_rejected()
    {
        var rejected = await service.UploadAsync(
            experienceId,
            "private.docx",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            8,
            new MemoryStream(new byte[8]));

        Assert.Equal(ExperienceAttachmentError.UnsupportedType, rejected.Error);
    }

    [Fact]
    public async Task Storage_directory_cannot_be_inside_web_root()
    {
        var environment = new TestWebHostEnvironment(root);
        var options = Options.Create(new ExperienceAttachmentStorageOptions
        {
            Directory = environment.WebRootPath
        });

        Assert.Throws<InvalidOperationException>(() => new ExperienceAttachmentStorageService(
            factory,
            environment,
            options,
            NullLogger<ExperienceAttachmentStorageService>.Instance));
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
