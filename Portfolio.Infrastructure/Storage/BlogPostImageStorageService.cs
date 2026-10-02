using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Portfolio.Application.Blog;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Storage;

public sealed class BlogPostImageStorageService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    IOptions<BlogPostImageStorageOptions> options) : IBlogPostImageService
{
    private const string PublicPathPrefix = "/blog-post-images/";
    private const string StoragePrefix = "posts";
    private readonly BlogPostImageStorageOptions settings = options.Value;
    private readonly string storageRoot = ResolveStorageRoot(environment, options.Value.Directory);

    public long MaximumFileSizeBytes => settings.MaximumFileSizeBytes;

    public async Task<IReadOnlyList<BlogPostImageReadModel>> GetImagesAsync(
        Guid blogPostId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var post = await context.BlogPosts.AsNoTracking()
            .Where(candidate => candidate.Id == blogPostId)
            .Select(candidate => new { candidate.FeaturedImagePath })
            .SingleOrDefaultAsync(cancellationToken);
        if (post is null)
        {
            return [];
        }

        var images = await context.BlogPostImages.AsNoTracking()
            .Where(image => image.BlogPostId == blogPostId)
            .OrderBy(image => image.CreatedAt)
            .ThenBy(image => image.Id)
            .ToListAsync(cancellationToken);
        return images.Select(image => ToReadModel(image, post.FeaturedImagePath)).ToArray();
    }

    public async Task<BlogPostImageOperationResult> UploadImageAsync(
        Guid blogPostId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedContentType = GetContentType(extension);
        if (expectedContentType is null
            || !settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
            || !string.Equals(expectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return new(BlogPostImageError.UnsupportedType);
        }

        if (fileSize <= 0 || fileSize > settings.MaximumFileSizeBytes)
        {
            return new(BlogPostImageError.TooLarge);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await context.BlogPosts.AnyAsync(post => post.Id == blogPostId, cancellationToken))
        {
            return new(BlogPostImageError.BlogPostNotFound);
        }

        var id = Guid.NewGuid();
        var storageKey = $"{StoragePrefix}/{blogPostId:N}/{id:N}{extension}";
        var destination = ResolveStoragePath(storageKey, blogPostId);
        var temporary = Path.Combine(storageRoot, $"{Guid.NewGuid():N}.upload");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
        try
        {
            await using (var output = new FileStream(
                temporary,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[81920];
                long copied = 0;
                int read;
                while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    copied += read;
                    if (copied > settings.MaximumFileSizeBytes)
                    {
                        return new(BlogPostImageError.TooLarge);
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (copied != fileSize)
                {
                    return new(BlogPostImageError.InvalidContent);
                }
            }

            if (!await HasValidContentAsync(temporary, extension, cancellationToken))
            {
                return new(BlogPostImageError.InvalidContent);
            }

            File.Move(temporary, destination);
            var image = new BlogPostImage
            {
                Id = id,
                BlogPostId = blogPostId,
                StorageKey = storageKey,
                ContentType = expectedContentType,
                SizeBytes = fileSize,
                CreatedAt = DateTimeOffset.UtcNow
            };
            context.BlogPostImages.Add(image);
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                File.Delete(destination);
                throw;
            }

            return new(BlogPostImageError.None, ToReadModel(image, null));
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public async Task<BlogPostImageOperationResult> SetFeaturedImageAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var image = await context.BlogPostImages
            .Include(candidate => candidate.BlogPost)
            .SingleOrDefaultAsync(candidate => candidate.Id == imageId, cancellationToken);
        if (image is null)
        {
            return new(BlogPostImageError.NotFound);
        }

        image.BlogPost.FeaturedImagePath = PublicPathPrefix + image.Id;
        await context.SaveChangesAsync(cancellationToken);
        return new(BlogPostImageError.None, ToReadModel(image, image.BlogPost.FeaturedImagePath));
    }

    public async Task<BlogPostImageError> DeleteImageAsync(
        Guid imageId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var image = await context.BlogPostImages
            .Include(candidate => candidate.BlogPost)
            .SingleOrDefaultAsync(candidate => candidate.Id == imageId, cancellationToken);
        if (image is null)
        {
            return BlogPostImageError.NotFound;
        }

        var imageUrl = PublicPathPrefix + image.Id;
        var administrativeImageUrl = "/admin" + imageUrl;
        var isReferenced = await context.BlogPostTranslations.AnyAsync(translation =>
            translation.BlogPostId == image.BlogPostId
            && (translation.Content.Contains(imageUrl) || translation.Content.Contains(administrativeImageUrl)),
            cancellationToken);
        if (isReferenced)
        {
            return BlogPostImageError.InUse;
        }

        var storedPath = TryGetStoredPath(image.StorageKey, image.BlogPostId, out var path) ? path : null;
        if (string.Equals(image.BlogPost.FeaturedImagePath, imageUrl, StringComparison.Ordinal))
        {
            image.BlogPost.FeaturedImagePath = null;
        }

        context.BlogPostImages.Remove(image);
        await context.SaveChangesAsync(cancellationToken);
        DeleteFile(storedPath);
        return BlogPostImageError.None;
    }

    public async Task<BlogPostImageContent?> OpenImageAsync(
        Guid imageId,
        bool administratorCanViewDrafts,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var image = await context.BlogPostImages.AsNoTracking()
            .Where(candidate => candidate.Id == imageId)
            .Select(candidate => new
            {
                candidate.StorageKey,
                candidate.ContentType,
                candidate.BlogPostId,
                candidate.BlogPost.EditorialStatus,
                candidate.BlogPost.PublishedOn
            })
            .SingleOrDefaultAsync(cancellationToken);
        if (image is null
            || (!administratorCanViewDrafts
                && (image.EditorialStatus != BlogPostEditorialStatus.Published
                    || image.PublishedOn is not { } publishedOn
                    || publishedOn > DateTimeOffset.UtcNow))
            || !TryGetStoredPath(image.StorageKey, image.BlogPostId, out var fullPath)
            || !File.Exists(fullPath))
        {
            return null;
        }

        return new(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read), image.ContentType);
    }

    private static string ResolveStorageRoot(IWebHostEnvironment environment, string directory)
    {
        var root = Path.GetFullPath(Path.IsPathRooted(directory)
            ? directory
            : Path.Combine(environment.ContentRootPath, directory));
        var webRoot = Path.GetFullPath(string.IsNullOrWhiteSpace(environment.WebRootPath)
            ? Path.Combine(environment.ContentRootPath, "wwwroot")
            : environment.WebRootPath);
        if (string.Equals(root, webRoot, StringComparison.OrdinalIgnoreCase)
            || root.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Blog image storage must be outside the web root.");
        }

        Directory.CreateDirectory(root);
        return root;
    }

    private string ResolveStoragePath(string storageKey, Guid blogPostId)
    {
        if (!TryGetStoredPath(storageKey, blogPostId, out var path))
        {
            throw new InvalidOperationException("The blog image storage key is invalid.");
        }

        return path;
    }

    private bool TryGetStoredPath(string storageKey, Guid blogPostId, out string fullPath)
    {
        fullPath = string.Empty;
        var prefix = $"{StoragePrefix}/{blogPostId:N}/";
        if (!storageKey.StartsWith(prefix, StringComparison.Ordinal)
            || storageKey[prefix.Length..].Contains('/')
            || storageKey[prefix.Length..].Contains('\\'))
        {
            return false;
        }

        var fileName = storageKey[prefix.Length..];
        if (fileName.Length != 32 + Path.GetExtension(fileName).Length
            || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _)
            || !settings.AllowedExtensions.Contains(Path.GetExtension(fileName), StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var root = Path.GetFullPath(Path.Combine(storageRoot, StoragePrefix, blogPostId.ToString("N")));
        fullPath = Path.GetFullPath(Path.Combine(root, fileName));
        return fullPath.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static BlogPostImageReadModel ToReadModel(BlogPostImage image, string? featuredImagePath) => new(
        image.Id,
        image.BlogPostId,
        image.ContentType,
        image.SizeBytes,
        image.CreatedAt,
        string.Equals(featuredImagePath, PublicPathPrefix + image.Id, StringComparison.Ordinal));

    private async Task<bool> HasValidContentAsync(
        string path,
        string extension,
        CancellationToken cancellationToken)
    {
        if (extension == ".svg")
        {
            return await IsSafeSvgAsync(path, cancellationToken);
        }

        var header = new byte[12];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var read = await stream.ReadAsync(header, cancellationToken);
        return extension switch
        {
            ".png" => read >= 8
                && header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47
                && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A,
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".webp" => read >= 12
                && System.Text.Encoding.ASCII.GetString(header, 0, 4) == "RIFF"
                && System.Text.Encoding.ASCII.GetString(header, 8, 4) == "WEBP",
            _ => false
        };
    }

    private static async Task<bool> IsSafeSvgAsync(string path, CancellationToken cancellationToken)
    {
        try
        {
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null,
                MaxCharactersInDocument = 10 * 1024 * 1024,
                Async = true
            };
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = XmlReader.Create(stream, settings);
            var document = await XDocument.LoadAsync(reader, LoadOptions.None, cancellationToken);
            var root = document.Root;
            if (root?.Name.LocalName != "svg"
                || root.Name.NamespaceName != "http://www.w3.org/2000/svg")
            {
                return false;
            }

            var blockedElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "script", "foreignObject", "iframe", "object", "embed", "audio", "video",
                "style", "animate", "animateMotion", "animateTransform", "set", "discard"
            };
            foreach (var element in document.Descendants())
            {
                if (blockedElements.Contains(element.Name.LocalName))
                {
                    return false;
                }

                foreach (var attribute in element.Attributes())
                {
                    if (attribute.IsNamespaceDeclaration)
                    {
                        continue;
                    }

                    var name = attribute.Name.LocalName;
                    var value = attribute.Value.Trim();
                    if (name.StartsWith("on", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("base", StringComparison.OrdinalIgnoreCase)
                        || name.Equals("href", StringComparison.OrdinalIgnoreCase) && !value.StartsWith('#')
                        || value.Contains("javascript:", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("data:", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("file:", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("http:", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("https:", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("@import", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("expression(", StringComparison.OrdinalIgnoreCase)
                        || value.Contains("url(", StringComparison.OrdinalIgnoreCase)
                            && !System.Text.RegularExpressions.Regex.IsMatch(
                                value,
                                "url\\(\\s*['\"]?#[-A-Za-z0-9_.]+['\"]?\\s*\\)",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        catch (Exception exception) when (exception is XmlException or InvalidOperationException)
        {
            return false;
        }
    }

    private static string? GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        ".svg" => "image/svg+xml",
        _ => null
    };

    private static void DeleteFile(string? path)
    {
        if (path is not null && File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
