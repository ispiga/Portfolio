using System.Globalization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Certifications;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Services;
using Portfolio.Web.Components.Admin;
using Xunit;

namespace Portfolio.Tests;

public sealed class CertificationAdministrationTests
{
    [Fact]
    public async Task Create_update_localized_content_and_preserve_order_and_spanish_fallback()
    {
        var factory = CreateFactory();
        var service = new CertificationAdministrationService(factory, new TestCertificationMediaService());
        var created = await service.SaveAsync(ValidRequest() with { DisplayOrder = 3 });

        Assert.True(created.Succeeded);
        var details = await service.GetCertificationAsync(created.CertificationId!.Value);
        Assert.NotNull(details);
        Assert.True(details.HasSpanishTranslation);
        Assert.True(details.HasEnglishTranslation);
        Assert.Equal("Certificación profesional", details.Spanish.Name);
        Assert.Equal("Entidad emisora", details.Spanish.Issuer);
        Assert.Equal("Detalles en español", details.Spanish.Details);
        Assert.Equal("Professional certification", details.English.Name);
        Assert.Equal("English details", details.English.Details);
        Assert.Equal(new DateOnly(2024, 5, 6), details.IssuedOn);
        Assert.Equal("https://example.test/credential", details.CredentialUrl);
        Assert.Equal("ABC-123", details.CredentialId);
        Assert.Equal(40, details.Hours);
        Assert.Equal(3, details.DisplayOrder);

        var updated = await service.SaveAsync(ValidRequest() with
        {
            Id = created.CertificationId,
            DisplayOrder = 1,
            English = EmptyTranslation()
        });

        Assert.True(updated.Succeeded);
        details = await service.GetCertificationAsync(created.CertificationId.Value);
        Assert.NotNull(details);
        Assert.False(details.HasEnglishTranslation);
        Assert.Equal(1, details.DisplayOrder);

        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var publicCertification = Assert.Single(await new CertificationQueryService(factory).GetCertificationsAsync());
            Assert.Equal("Certificación profesional", publicCertification.Name);
            Assert.Equal("Detalles en español", publicCertification.Details);
            Assert.Equal("ABC-123", publicCertification.CredentialId);
            Assert.Equal(40, publicCertification.Hours);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Admin_and_public_queries_keep_manual_order()
    {
        var factory = CreateFactory();
        var service = new CertificationAdministrationService(factory, new TestCertificationMediaService());
        var later = await service.SaveAsync(ValidRequest() with { DisplayOrder = 8 });
        var first = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Otra certificación", "Otra entidad"),
            English = EmptyTranslation(),
            DisplayOrder = 2
        });

        Assert.Equal(new[] { first.CertificationId, later.CertificationId },
            (await service.GetCertificationsAsync()).Select(item => (Guid?)item.Id));
        Assert.Equal(new[] { first.CertificationId, later.CertificationId },
            (await new CertificationQueryService(factory).GetCertificationsAsync()).Select(item => (Guid?)item.Id));
    }

    [Fact]
    public async Task Invalid_order_url_and_partial_translations_are_rejected()
    {
        var service = new CertificationAdministrationService(CreateFactory(), new TestCertificationMediaService());
        var result = await service.SaveAsync(ValidRequest() with
        {
            DisplayOrder = -1,
            CredentialUrl = "javascript:alert(1)",
            Hours = -1,
            CredentialId = new string('x', 201),
            English = new("Partial translation", string.Empty)
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.ResourceKey == "CertificationDisplayOrderInvalid");
        Assert.Contains(result.Errors, error => error.ResourceKey == "CertificationUrlInvalid");
        Assert.Contains(result.Errors, error => error.Field == "Hours" && error.ResourceKey == "CertificationHoursInvalid");
        Assert.Contains(result.Errors, error => error.Field == "CredentialId" && error.ResourceKey == "CertificationFieldTooLong");
        Assert.Contains(result.Errors, error => error.Field == "English.Issuer");
    }

    [Fact]
    public async Task Details_longer_than_one_thousand_characters_are_rejected()
    {
        var service = new CertificationAdministrationService(CreateFactory(), new TestCertificationMediaService());
        var result = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Certificación profesional", "Entidad emisora", new string('x', 1001))
        });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.Field == "Spanish.Details");
    }

    [Fact]
    public async Task Delete_removes_certification_and_translations_and_cleans_media()
    {
        var factory = CreateFactory();
        var media = new TestCertificationMediaService();
        var service = new CertificationAdministrationService(factory, media);
        var created = await service.SaveAsync(ValidRequest());
        var id = created.CertificationId!.Value;
        await using (var attachmentContext = factory.CreateDbContext())
        {
            var certification = await attachmentContext.Certifications.SingleAsync(item => item.Id == id);
            certification.ImagePath = $"certifications/{id:N}/card/image.png";
            await attachmentContext.SaveChangesAsync();
        }

        Assert.Equal(CertificationDeleteResult.Deleted, await service.DeleteAsync(id));
        Assert.Equal(CertificationDeleteResult.NotFound, await service.DeleteAsync(id));
        Assert.Equal(1, media.DeleteCertificationFilesCallCount);
        await using var context = factory.CreateDbContext();
        Assert.Empty(await context.Certifications
            .SelectMany(item => item.Translations)
            .ToListAsync());
    }

    [Fact]
    public async Task Delete_requires_attachments_to_be_removed_but_not_the_card_image()
    {
        var factory = CreateFactory();
        var media = new TestCertificationMediaService();
        var service = new CertificationAdministrationService(factory, media);
        var created = await service.SaveAsync(ValidRequest());
        var id = created.CertificationId!.Value;
        await using (var attachmentContext = factory.CreateDbContext())
        {
            attachmentContext.CertificationAttachments.Add(new CertificationAttachment
            {
                Id = Guid.NewGuid(),
                CertificationId = id,
                OriginalFileName = "certificate.pdf",
                DisplayName = "certificate.pdf",
                StorageKey = $"certifications/{id:N}/attachments/file.pdf",
                ContentType = "application/pdf",
                SizeBytes = 100,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await attachmentContext.SaveChangesAsync();
        }

        Assert.Equal(CertificationDeleteResult.HasAttachments, await service.DeleteAsync(id));
        Assert.Equal(0, media.DeleteCertificationFilesCallCount);
        await using (var cleanupContext = factory.CreateDbContext())
        {
            Assert.True(await cleanupContext.Certifications.AnyAsync(item => item.Id == id));
            cleanupContext.CertificationAttachments.RemoveRange(cleanupContext.CertificationAttachments.Where(item => item.CertificationId == id));
            await cleanupContext.SaveChangesAsync();
        }

        Assert.Equal(CertificationDeleteResult.Deleted, await service.DeleteAsync(id));
        Assert.Equal(1, media.DeleteCertificationFilesCallCount);
    }

    [Fact]
    public void Admin_list_and_editor_require_administrator_policy()
    {
        Assert.Equal(PortfolioAuthorization.AdministratorPolicy,
            typeof(Certifications).GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>().Single().Policy);
        Assert.Equal(PortfolioAuthorization.AdministratorPolicy,
            typeof(CertificationEditor).GetCustomAttributes(typeof(AuthorizeAttribute), false)
                .Cast<AuthorizeAttribute>().Single().Policy);
    }

    private static CertificationEditRequest ValidRequest() => new(
        null,
        new DateOnly(2024, 5, 6),
        "https://example.test/credential",
        "ABC-123",
        40,
        0,
        new("Certificación profesional", "Entidad emisora", "Detalles en español"),
        new("Professional certification", "Issuing organization", "English details"));

    private static CertificationTranslationInput EmptyTranslation() => new(string.Empty, string.Empty);

    private static TestContextFactory CreateFactory() => new(Guid.NewGuid().ToString());

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }

    private sealed class TestCertificationMediaService : ICertificationMediaService
    {
        public long MaximumFileSizeBytes => 10 * 1024 * 1024;
        public int DeleteCertificationFilesCallCount { get; private set; }

        public Task<IReadOnlyList<CertificationAttachmentReadModel>> GetAttachmentsAsync(Guid certificationId, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<CertificationAttachmentReadModel>>([]);
        public Task<CertificationAttachmentOperationResult> UploadAttachmentAsync(Guid certificationId, string fileName, string contentType, long fileSize, Stream content, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<bool> UpdateAttachmentDisplayNameAsync(Guid attachmentId, string displayName, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task<bool> SetAttachmentPublicAsync(Guid attachmentId, bool isPublic, CancellationToken cancellationToken = default) =>
            Task.FromResult(false);
        public Task<bool> DeleteAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<CertificationAttachmentContent?> OpenAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default) => Task.FromResult<CertificationAttachmentContent?>(null);
        public Task<CertificationAttachmentContent?> OpenPublicAttachmentAsync(Guid attachmentId, CancellationToken cancellationToken = default) => Task.FromResult<CertificationAttachmentContent?>(null);
        public Task<CertificationCardImageOperationResult> UploadCardImageAsync(Guid certificationId, string fileName, string contentType, long fileSize, Stream content, CancellationToken cancellationToken = default) =>
            throw new NotSupportedException();
        public Task<bool> DeleteCardImageAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<CertificationImageContent?> OpenCardImageAsync(Guid certificationId, CancellationToken cancellationToken = default) => Task.FromResult<CertificationImageContent?>(null);
        public Task DeleteCertificationFilesAsync(Guid certificationId, CancellationToken cancellationToken = default)
        {
            DeleteCertificationFilesCallCount++;
            return Task.CompletedTask;
        }

        public Task CleanupCertificationDirectoryAsync(Guid certificationId, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
