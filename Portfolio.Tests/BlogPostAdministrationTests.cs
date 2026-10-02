using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Blog;
using Portfolio.Domain.Entities;
using Portfolio.Infrastructure;
using Portfolio.Infrastructure.Identity;
using Portfolio.Infrastructure.Services;
using Portfolio.Web.Authentication;
using Portfolio.Web.Components.Admin;
using Xunit;

namespace Portfolio.Tests;

public sealed class BlogPostAdministrationTests
{
    [Fact]
    public async Task Create_draft_with_missing_translation_and_update_state_and_translations()
    {
        var factory = CreateFactory();
        var service = new BlogPostAdministrationService(factory);
        var created = await service.SaveAsync(ValidRequest() with
        {
            EditorialStatus = BlogPostEditorialStatus.Draft,
            English = EmptyTranslation()
        });

        Assert.True(created.Succeeded);
        var details = await service.GetBlogPostAsync(created.BlogPostId!.Value);
        Assert.NotNull(details);
        Assert.Equal(BlogPostEditorialStatus.Draft, details.EditorialStatus);
        Assert.Null(details.PublishedOn);
        Assert.True(details.HasSpanishTranslation);
        Assert.False(details.HasEnglishTranslation);

        var published = await service.SaveAsync(ValidRequest() with
        {
            Id = created.BlogPostId,
            EditorialStatus = BlogPostEditorialStatus.Published,
            English = EmptyTranslation()
        });
        Assert.True(published.Succeeded);
        details = await service.GetBlogPostAsync(created.BlogPostId.Value);
        Assert.NotNull(details);
        Assert.Equal(BlogPostEditorialStatus.Published, details.EditorialStatus);
        Assert.InRange(details.PublishedOn!.Value, DateTimeOffset.UtcNow.AddSeconds(-5), DateTimeOffset.UtcNow);
        Assert.False(details.HasEnglishTranslation);

        var publicationDate = details.PublishedOn;
        var edited = await service.SaveAsync(ValidRequest() with { Id = created.BlogPostId });
        Assert.True(edited.Succeeded);
        details = await service.GetBlogPostAsync(created.BlogPostId.Value);
        Assert.Equal(publicationDate, details!.PublishedOn);
        Assert.True(details.HasEnglishTranslation);
    }

    [Fact]
    public async Task Ready_to_publish_requires_complete_spanish_but_draft_can_be_incomplete()
    {
        var factory = CreateFactory();
        var service = new BlogPostAdministrationService(factory);
        var draft = await service.SaveAsync(ValidRequest() with
        {
            EditorialStatus = BlogPostEditorialStatus.Draft,
            Spanish = EmptyTranslation(),
            English = EmptyTranslation()
        });
        Assert.True(draft.Succeeded);

        var notReady = await service.SaveAsync(ValidRequest() with
        {
            EditorialStatus = BlogPostEditorialStatus.ReadyToPublish,
            Spanish = new("", "", "", "")
        });
        Assert.False(notReady.Succeeded);
        Assert.Contains(notReady.Errors, error => error.Field == "Spanish.Title");
        Assert.Contains(notReady.Errors, error => error.Field == "Spanish.Content");
    }

    [Fact]
    public async Task Duplicate_slugs_are_rejected_per_language_and_translation_limits_are_validated()
    {
        var service = new BlogPostAdministrationService(CreateFactory());
        var first = await service.SaveAsync(ValidRequest());
        var duplicate = await service.SaveAsync(ValidRequest() with
        {
            Spanish = ValidRequest().Spanish with { Title = "Otro artículo" },
            English = EmptyTranslation()
        });
        var tooLong = await service.SaveAsync(ValidRequest() with
        {
            Spanish = ValidRequest().Spanish with { Excerpt = new string('x', 501) }
        });

        Assert.True(first.Succeeded);
        Assert.False(duplicate.Succeeded);
        Assert.Contains(duplicate.Errors, error => error.Field == "Spanish.Slug" && error.ResourceKey == "BlogPostSlugDuplicate");
        Assert.False(tooLong.Succeeded);
        Assert.Contains(tooLong.Errors, error => error.Field == "Spanish.Excerpt");
    }

    [Fact]
    public async Task Home_query_uses_only_published_posts_with_non_future_dates_and_keeps_fallback()
    {
        var factory = CreateFactory();
        var spanishOnly = ValidPost(Guid.NewGuid(), DateTimeOffset.UtcNow.AddMinutes(-2), BlogPostEditorialStatus.Published, isFeatured: true);
        var draft = ValidPost(Guid.NewGuid(), DateTimeOffset.UtcNow, BlogPostEditorialStatus.Draft, isFeatured: true);
        var ready = ValidPost(Guid.NewGuid(), DateTimeOffset.UtcNow, BlogPostEditorialStatus.ReadyToPublish, isFeatured: true);
        var future = ValidPost(Guid.NewGuid(), DateTimeOffset.UtcNow.AddDays(1), BlogPostEditorialStatus.Published, isFeatured: true);
        await using (var context = factory.CreateDbContext())
        {
            context.BlogPosts.AddRange(spanishOnly, draft, ready, future);
            await context.SaveChangesAsync();
        }

        var post = await new BlogPostQueryService(factory).GetFeaturedPostAsync();
        Assert.NotNull(post);
        Assert.Equal(spanishOnly.Id, post.Id);

        var previousCulture = System.Globalization.CultureInfo.CurrentUICulture;
        try
        {
            System.Globalization.CultureInfo.CurrentUICulture = System.Globalization.CultureInfo.GetCultureInfo("en-US");
            post = await new BlogPostQueryService(factory).GetFeaturedPostAsync();
            Assert.NotNull(post);
            Assert.Equal("Artículo", post.Title);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentUICulture = previousCulture;
        }
    }

    [Fact]
    public async Task Delete_requires_images_to_be_removed_first_and_cleans_translations()
    {
        var factory = CreateFactory();
        var service = new BlogPostAdministrationService(factory);
        var created = await service.SaveAsync(ValidRequest());
        var postId = created.BlogPostId!.Value;
        await using (var context = factory.CreateDbContext())
        {
            context.BlogPostImages.Add(new BlogPostImage
            {
                Id = Guid.NewGuid(),
                BlogPostId = postId,
                StorageKey = $"posts/{postId:N}/image.png",
                ContentType = "image/png",
                SizeBytes = 100,
                CreatedAt = DateTimeOffset.UtcNow
            });
            await context.SaveChangesAsync();
        }

        Assert.Equal(BlogPostDeleteResult.HasImages, await service.DeleteAsync(postId));
        await using (var context = factory.CreateDbContext())
        {
            context.BlogPostImages.RemoveRange(context.BlogPostImages.Where(image => image.BlogPostId == postId));
            await context.SaveChangesAsync();
        }

        Assert.Equal(BlogPostDeleteResult.Deleted, await service.DeleteAsync(postId));
        Assert.Equal(BlogPostDeleteResult.NotFound, await service.DeleteAsync(postId));
        await using var check = factory.CreateDbContext();
        Assert.Empty(await check.BlogPostTranslations.ToListAsync());
    }

    [Fact]
    public void Blog_admin_pages_and_preview_endpoint_require_administrator_for_admin_operations()
    {
        Assert.Equal(PortfolioAuthorization.AdministratorPolicy,
            typeof(BlogPosts).GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        Assert.Equal(PortfolioAuthorization.AdministratorPolicy,
            typeof(BlogPostEditor).GetCustomAttribute<AuthorizeAttribute>()?.Policy);
        var endpoint = typeof(BlogPostImageEndpoints).GetMethod(nameof(BlogPostImageEndpoints.MapBlogPostImageEndpoints));
        Assert.NotNull(endpoint);
    }

    private static BlogPostEditRequest ValidRequest() => new(
        null,
        BlogPostEditorialStatus.ReadyToPublish,
        true,
        new("Artículo", "articulo", "Extracto", "<p>Contenido</p>"),
        new("Article", "article", "Excerpt", "<p>Content</p>"));

    private static BlogPostTranslationInput EmptyTranslation() => new("", "", "", "");

    private static BlogPost ValidPost(Guid id, DateTimeOffset publishedOn, BlogPostEditorialStatus status, bool isFeatured) => new()
    {
        Id = id,
        EditorialStatus = status,
        PublishedOn = publishedOn,
        IsFeatured = isFeatured,
        Translations =
        [
            new BlogPostTranslation
            {
                LanguageCode = "es-ES",
                Title = "Artículo",
                Slug = id.ToString("N"),
                Excerpt = "Resumen",
                Content = "Contenido"
            }
        ]
    };

    private static TestContextFactory CreateFactory() => new(Guid.NewGuid().ToString());

    private sealed class TestContextFactory(string databaseName) : IDbContextFactory<PortfolioDbContext>
    {
        private readonly DbContextOptions<PortfolioDbContext> options = new DbContextOptionsBuilder<PortfolioDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        public PortfolioDbContext CreateDbContext() => new(options);
    }
}
