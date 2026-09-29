using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Experiences;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Services;
using Xunit;

namespace Portfolio.Tests;

public sealed class ExperienceAdministrationTests
{
    [Fact]
    public async Task Create_and_edit_persist_both_localizations_and_order()
    {
        var factory = CreateFactory();
        var service = new ExperienceAdministrationService(factory);
        var request = ValidRequest() with { DisplayOrder = 3 };

        var created = await service.SaveAsync(request);

        Assert.True(created.Succeeded);
        var details = await service.GetExperienceAsync(created.ExperienceId!.Value);
        Assert.NotNull(details);
        Assert.True(details.HasSpanishTranslation);
        Assert.True(details.HasEnglishTranslation);
        Assert.Equal("Desarrolladora", details.Spanish.RoleTitle);
        Assert.Equal("Software developer", details.English.RoleTitle);
        Assert.Equal(3, details.DisplayOrder);

        var updated = await service.SaveAsync(request with
        {
            Id = created.ExperienceId,
            DisplayOrder = 1,
            English = new("Senior software developer", "Example Ltd", "English summary")
        });

        Assert.True(updated.Succeeded);
        details = await service.GetExperienceAsync(created.ExperienceId.Value);
        Assert.NotNull(details);
        Assert.Equal(1, details.DisplayOrder);
        Assert.Equal("Senior software developer", details.English.RoleTitle);
    }

    [Fact]
    public async Task Missing_english_translation_is_allowed_and_reported_while_public_uses_spanish_fallback()
    {
        var factory = CreateFactory();
        var service = new ExperienceAdministrationService(factory);
        var result = await service.SaveAsync(ValidRequest() with { English = EmptyTranslation() });

        Assert.True(result.Succeeded);
        var details = await service.GetExperienceAsync(result.ExperienceId!.Value);
        Assert.NotNull(details);
        Assert.True(details.HasSpanishTranslation);
        Assert.False(details.HasEnglishTranslation);

        var previousCulture = CultureInfo.CurrentUICulture;
        try
        {
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
            var publicService = new ExperienceQueryService(factory);
            var publicExperience = Assert.Single(await publicService.GetExperiencesAsync());
            Assert.Equal("Desarrolladora", publicExperience.RoleTitle);
        }
        finally
        {
            CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Admin_list_and_public_query_are_sorted_by_display_order()
    {
        var factory = CreateFactory();
        var service = new ExperienceAdministrationService(factory);
        var second = await service.SaveAsync(ValidRequest() with { DisplayOrder = 2 });
        var first = await service.SaveAsync(ValidRequest() with { DisplayOrder = 1 });

        var adminResults = await service.GetExperiencesAsync();
        Assert.Equal(
            new[] { first.ExperienceId!.Value, second.ExperienceId!.Value },
            adminResults.Select(item => item.Id).ToArray());

        var publicResults = await new ExperienceQueryService(factory).GetExperiencesAsync();
        Assert.Equal(
            new[] { first.ExperienceId!.Value, second.ExperienceId!.Value },
            publicResults.Select(item => item.Id).ToArray());
    }

    [Fact]
    public async Task Invalid_dates_order_and_partial_translations_are_rejected_server_side()
    {
        var factory = CreateFactory();
        var service = new ExperienceAdministrationService(factory);
        var request = ValidRequest() with
        {
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2024, 1, 1),
            DisplayOrder = -1,
            English = new("Developer", string.Empty, "Summary")
        };

        var result = await service.SaveAsync(request);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error => error.ResourceKey == "ExperienceEndDateBeforeStart");
        Assert.Contains(result.Errors, error => error.ResourceKey == "ExperienceDisplayOrderInvalid");
        Assert.Contains(result.Errors, error => error.Field == "English.CompanyName");
        await using var context = factory.CreateDbContext();
        Assert.Empty(await context.Experiences.ToListAsync());
    }

    [Fact]
    public async Task Missing_start_date_is_rejected_server_side()
    {
        var service = new ExperienceAdministrationService(CreateFactory());

        var result = await service.SaveAsync(ValidRequest() with { StartDate = default });

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, error =>
            error.Field == "StartDate" && error.ResourceKey == "ExperienceStartDateRequired");
    }

    [Fact]
    public async Task Description_accepts_two_thousand_characters_and_rejects_longer_values()
    {
        var service = new ExperienceAdministrationService(CreateFactory());
        var description = new string('a', ExperienceEditValidator.MaximumSummaryLength);

        var valid = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Desarrolladora", "Empresa de ejemplo", description)
        });
        var invalid = await service.SaveAsync(ValidRequest() with
        {
            Spanish = new("Desarrolladora", "Empresa de ejemplo", description + "a")
        });

        Assert.True(valid.Succeeded);
        Assert.False(invalid.Succeeded);
        Assert.Contains(invalid.Errors, error => error.Field == "Spanish.Summary");
    }

    [Fact]
    public async Task Deleting_experience_with_attachments_is_blocked()
    {
        var factory = CreateFactory();
        var service = new ExperienceAdministrationService(factory);
        var created = await service.SaveAsync(ValidRequest());
        await using (var context = factory.CreateDbContext())
        {
            context.ExperienceAttachments.Add(new ExperienceAttachment
            {
                Id = Guid.NewGuid(),
                ExperienceId = created.ExperienceId!.Value,
                OriginalFileName = "private.pdf",
                StorageKey = "experience/private.pdf",
                ContentType = "application/pdf",
                SizeBytes = 20,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        var result = await service.DeleteAsync(created.ExperienceId!.Value);

        Assert.Equal(ExperienceDeleteResult.HasAttachments, result);
    }

    [Fact]
    public async Task Public_query_includes_only_explicitly_published_attachments()
    {
        var factory = CreateFactory();
        var service = new ExperienceAdministrationService(factory);
        var created = await service.SaveAsync(ValidRequest());
        await using (var context = factory.CreateDbContext())
        {
            context.ExperienceAttachments.AddRange(
                new ExperienceAttachment
                {
                    Id = Guid.NewGuid(),
                    ExperienceId = created.ExperienceId!.Value,
                    OriginalFileName = "public-certificate.pdf",
                    StorageKey = "experience/public.pdf",
                    ContentType = "application/pdf",
                    SizeBytes = 20,
                    CreatedAt = DateTimeOffset.UtcNow,
                    IsPublic = true
                },
                new ExperienceAttachment
                {
                    Id = Guid.NewGuid(),
                    ExperienceId = created.ExperienceId.Value,
                    OriginalFileName = "private-letter.pdf",
                    StorageKey = "experience/private.pdf",
                    ContentType = "application/pdf",
                    SizeBytes = 20,
                    CreatedAt = DateTimeOffset.UtcNow.AddSeconds(1),
                    IsPublic = false
                });
            await context.SaveChangesAsync();
        }

        var result = Assert.Single(await new ExperienceQueryService(factory).GetExperiencesAsync());

        Assert.Equal("public-certificate.pdf", Assert.Single(result.Attachments!).OriginalFileName);
    }

    private static ExperienceEditRequest ValidRequest() => new(
        null,
        new DateOnly(2021, 3, 1),
        null,
        0,
        new("Desarrolladora", "Empresa de ejemplo", "Resumen en español"),
        new("Software developer", "Example Ltd", "English summary"));

    private static ExperienceTranslationInput EmptyTranslation() => new(string.Empty, string.Empty, string.Empty);

    private static TestContextFactory CreateFactory() => new(Guid.NewGuid().ToString());

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}
