using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.HomeContent;

namespace Portfolio.Infrastructure.Storage;

public sealed class HomeContentImageStorageService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    IOptions<HomeContentImageStorageOptions> options,
    ILogger<HomeContentImageStorageService> logger) : IHomeContentImageStorageService
{
    private readonly HomeContentImageStorageOptions settings = options.Value;
    private readonly string storageRoot = ResolveStorageRoot(environment, options.Value.Directory);

    public long MaximumFileSizeBytes => settings.MaximumFileSizeBytes;

    public async Task<HomeContentImageUploadResult> UploadAsync(
        HomeContentImageKind kind,
        Guid entityId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        if (entityId == Guid.Empty)
        {
            return new(HomeContentImageError.NotFound);
        }

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedContentType = GetContentType(extension);
        if (expectedContentType is null
            || !settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
            || !string.Equals(expectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return new(HomeContentImageError.UnsupportedType);
        }

        if (fileSize <= 0 || fileSize > settings.MaximumFileSizeBytes)
        {
            return new(HomeContentImageError.TooLarge);
        }

        var folder = GetFolderName(kind);
        if (folder is null)
        {
            return new(HomeContentImageError.NotFound);
        }

        var entityDirectory = Path.Combine(storageRoot, folder, entityId.ToString("N"));
        Directory.CreateDirectory(entityDirectory);
        var key = $"{folder}/{entityId:N}/{Guid.NewGuid():N}{extension}";
        var destination = ResolvePath(key, kind, entityId);
        var temporary = Path.Combine(entityDirectory, $"{Guid.NewGuid():N}.upload");
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
                        return new(HomeContentImageError.TooLarge);
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (copied != fileSize)
                {
                    return new(HomeContentImageError.InvalidContent);
                }
            }

            if (!await HasValidContentAsync(temporary, extension, cancellationToken))
            {
                return new(HomeContentImageError.InvalidContent);
            }

            File.Move(temporary, destination);
            return new(HomeContentImageError.None, key, expectedContentType, fileSize);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public Task DeleteAsync(HomeContentImageKind kind, Guid entityId, string? storageKey)
    {
        if (storageKey is null || !TryResolvePath(storageKey, kind, entityId, out var fullPath))
        {
            return Task.CompletedTask;
        }

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            var directory = Path.GetDirectoryName(fullPath)!;
            if (Directory.Exists(directory) && !Directory.EnumerateFileSystemEntries(directory).Any())
            {
                Directory.Delete(directory);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Unable to delete home content image {StorageKey}.", storageKey);
        }

        return Task.CompletedTask;
    }

    public async Task<HomeContentImageContent?> OpenPublicReadAsync(
        HomeContentImageKind kind,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        if (entityId == Guid.Empty)
        {
            return null;
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var image = kind switch
        {
            HomeContentImageKind.TechnologyLogo => await context.TechnologyLogos.AsNoTracking()
                .Where(logo => logo.Id == entityId)
                .Select(logo => new { logo.StorageKey, logo.ContentType })
                .SingleOrDefaultAsync(cancellationToken),
            HomeContentImageKind.Hobby => await context.Hobbies.AsNoTracking()
                .Where(hobby => hobby.Id == entityId)
                .Select(hobby => new { hobby.StorageKey, hobby.ContentType })
                .SingleOrDefaultAsync(cancellationToken),
            HomeContentImageKind.HeroProfile => await context.HeroContents.AsNoTracking()
                .Where(hero => hero.Id == 1 && hero.ProfileImageId == entityId)
                .Select(hero => new { StorageKey = hero.ProfileImageStorageKey!, ContentType = hero.ProfileImageContentType! })
                .SingleOrDefaultAsync(cancellationToken),
            _ => null
        };
        if (image is null
            || !TryResolvePath(image.StorageKey, kind, entityId, out var fullPath)
            || !File.Exists(fullPath)
            || !string.Equals(GetContentType(Path.GetExtension(fullPath)), image.ContentType, StringComparison.Ordinal))
        {
            return null;
        }

        return new HomeContentImageContent(
            new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read),
            image.ContentType);
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
            throw new InvalidOperationException("Home content image storage must be outside the web root.");
        }

        Directory.CreateDirectory(root);
        return root;
    }

    private static string? GetFolderName(HomeContentImageKind kind) => kind switch
    {
        HomeContentImageKind.TechnologyLogo => "TechnologyLogos",
        HomeContentImageKind.Hobby => "Hobbies",
        HomeContentImageKind.HeroProfile => "HeroProfiles",
        _ => null
    };

    private string ResolvePath(string key, HomeContentImageKind kind, Guid entityId)
    {
        if (!TryResolvePath(key, kind, entityId, out var path))
        {
            throw new InvalidOperationException("The home content image storage key is invalid.");
        }

        return path;
    }

    private bool TryResolvePath(string key, HomeContentImageKind kind, Guid entityId, out string fullPath)
    {
        fullPath = string.Empty;
        var folder = GetFolderName(kind);
        if (folder is null || entityId == Guid.Empty)
        {
            return false;
        }

        var prefix = $"{folder}/{entityId:N}/";
        if (!key.StartsWith(prefix, StringComparison.Ordinal)
            || key[prefix.Length..].Contains('/')
            || key[prefix.Length..].Contains('\\'))
        {
            return false;
        }

        var fileName = key[prefix.Length..];
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (fileName.Length != 32 + extension.Length
            || !Guid.TryParseExact(Path.GetFileNameWithoutExtension(fileName), "N", out _)
            || !settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        var entityRoot = Path.GetFullPath(Path.Combine(storageRoot, folder, entityId.ToString("N")));
        fullPath = Path.GetFullPath(Path.Combine(entityRoot, fileName));
        return fullPath.StartsWith(entityRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private static string? GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".svg" => "image/svg+xml",
        _ => null
    };

    private static async Task<bool> HasValidContentAsync(string path, string extension, CancellationToken cancellationToken)
    {
        if (extension == ".svg")
        {
            return await SafeSvgDocument.IsSafeFileAsync(path, cancellationToken);
        }

        var header = new byte[8];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var read = await stream.ReadAsync(header, cancellationToken);
        return extension switch
        {
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read == 8 && header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            _ => false
        };
    }

}