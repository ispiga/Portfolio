using Microsoft.EntityFrameworkCore;
using Portfolio.Application.Blog;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Services;

public sealed class BlogPostAdministrationService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory) : IBlogPostAdministrationService
{
    public async Task<IReadOnlyList<BlogPostAdminListItem>> GetBlogPostsAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var posts = await context.BlogPosts
            .AsNoTracking()
            .AsSplitQuery()
            .Include(post => post.Translations)
            .Include(post => post.Images)
            .OrderByDescending(post => post.PublishedOn)
            .ThenBy(post => post.Id)
            .ToListAsync(cancellationToken);

        return posts.Select(post => new BlogPostAdminListItem(
            post.Id,
            FindTranslation(post, "es-ES")?.Title ?? string.Empty,
            post.EditorialStatus,
            post.PublishedOn,
            post.IsFeatured,
            IsComplete(FindTranslation(post, "es-ES")),
            IsComplete(FindTranslation(post, "en-US")),
            post.Images.Count)).ToArray();
    }

    public async Task<BlogPostAdminDetails?> GetBlogPostAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var post = await context.BlogPosts
            .AsNoTracking()
            .Include(candidate => candidate.Translations)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        return post is null ? null : ToDetails(post);
    }

    public async Task<BlogPostSaveResult> SaveAsync(
        BlogPostEditRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = BlogPostEditValidator.Validate(request).ToList();
        if (errors.Count > 0)
        {
            return new(false, null, errors);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        foreach (var (languageCode, translation) in new[]
        {
            ("es-ES", request.Spanish),
            ("en-US", request.English)
        })
        {
            if (string.IsNullOrWhiteSpace(translation.Slug))
            {
                continue;
            }

            var duplicateSlug = await context.BlogPostTranslations.AnyAsync(candidate =>
                candidate.LanguageCode == languageCode
                && candidate.Slug == translation.Slug.Trim()
                && candidate.BlogPostId != request.Id,
                cancellationToken);
            if (duplicateSlug)
            {
                errors.Add(new($"{(languageCode == "es-ES" ? "Spanish" : "English")}.Slug", "BlogPostSlugDuplicate"));
            }
        }

        if (errors.Count > 0)
        {
            return new(false, null, errors);
        }

        BlogPost post;
        if (request.Id is { } id)
        {
            var existing = await context.BlogPosts
                .Include(candidate => candidate.Translations)
                .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
            if (existing is null)
            {
                return new(false, null, [new(nameof(request.Id), "BlogPostNotFound")]);
            }

            post = existing;
        }
        else
        {
            post = new BlogPost { Id = Guid.NewGuid() };
            context.BlogPosts.Add(post);
        }

        if (request.EditorialStatus == BlogPostEditorialStatus.Published
            && post.EditorialStatus != BlogPostEditorialStatus.Published)
        {
            post.PublishedOn = DateTimeOffset.UtcNow;
        }

        post.EditorialStatus = request.EditorialStatus;
        post.IsFeatured = request.IsFeatured;
        UpdateTranslation(post, "es-ES", request.Spanish);
        UpdateTranslation(post, "en-US", request.English);

        await context.SaveChangesAsync(cancellationToken);
        return new(true, post.Id, []);
    }

    public async Task<BlogPostDeleteResult> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var post = await context.BlogPosts
            .Include(candidate => candidate.Translations)
            .Include(candidate => candidate.Images)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
        if (post is null)
        {
            return BlogPostDeleteResult.NotFound;
        }

        if (post.Images.Count > 0)
        {
            return BlogPostDeleteResult.HasImages;
        }

        context.BlogPostTranslations.RemoveRange(post.Translations);
        context.BlogPosts.Remove(post);
        await context.SaveChangesAsync(cancellationToken);
        return BlogPostDeleteResult.Deleted;
    }

    private static BlogPostAdminDetails ToDetails(BlogPost post)
    {
        var spanish = FindTranslation(post, "es-ES");
        var english = FindTranslation(post, "en-US");
        return new BlogPostAdminDetails(
            post.Id,
            post.EditorialStatus,
            post.PublishedOn,
            post.IsFeatured,
            post.FeaturedImagePath,
            ToInput(spanish),
            ToInput(english),
            IsComplete(spanish),
            IsComplete(english));
    }

    private static BlogPostTranslation? FindTranslation(BlogPost post, string languageCode) =>
        post.Translations.SingleOrDefault(translation => translation.LanguageCode == languageCode);

    private static bool IsComplete(BlogPostTranslation? translation) =>
        translation is not null
        && !string.IsNullOrWhiteSpace(translation.Title)
        && !string.IsNullOrWhiteSpace(translation.Slug)
        && !string.IsNullOrWhiteSpace(translation.Excerpt)
        && !string.IsNullOrWhiteSpace(translation.Content);

    private static BlogPostTranslationInput ToInput(BlogPostTranslation? translation) => new(
        translation?.Title ?? string.Empty,
        translation?.Slug ?? string.Empty,
        translation?.Excerpt ?? string.Empty,
        translation?.Content ?? string.Empty,
        translation?.FeaturedImageAlt);

    private static void UpdateTranslation(
        BlogPost post,
        string languageCode,
        BlogPostTranslationInput input)
    {
        var translation = FindTranslation(post, languageCode);
        if (IsEmpty(input))
        {
            if (translation is not null)
            {
                post.Translations.Remove(translation);
            }

            return;
        }

        if (translation is null)
        {
            translation = new BlogPostTranslation
            {
                BlogPostId = post.Id,
                LanguageCode = languageCode
            };
            post.Translations.Add(translation);
        }

        translation.Title = input.Title.Trim();
        translation.Slug = input.Slug.Trim();
        translation.Excerpt = input.Excerpt.Trim();
        translation.Content = input.Content.Trim();
        translation.FeaturedImageAlt = NormalizeOptional(input.FeaturedImageAlt);
    }

    private static bool IsEmpty(BlogPostTranslationInput input) =>
        string.IsNullOrWhiteSpace(input.Title)
        && string.IsNullOrWhiteSpace(input.Slug)
        && string.IsNullOrWhiteSpace(input.Excerpt)
        && string.IsNullOrWhiteSpace(input.Content)
        && string.IsNullOrWhiteSpace(input.FeaturedImageAlt);

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
