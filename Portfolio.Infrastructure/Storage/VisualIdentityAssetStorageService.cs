using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.VisualIdentity;

namespace Portfolio.Infrastructure.Storage;

public sealed class VisualIdentityAssetStorageService(
    IWebHostEnvironment environment,
    IOptions<VisualIdentityStorageOptions> options,
    ILogger<VisualIdentityAssetStorageService> logger) : IVisualIdentityAssetStorageService
{
    private static readonly Guid StorageDirectoryId = Guid.Parse("4b967221-3d42-4de9-9054-8293303329c8");
    private const string MainLogoResource = "Portfolio.Infrastructure.Assets.logo_portfolio.svg";
    private const string DefaultFaviconResource = "Portfolio.Infrastructure.Assets.default-favicon.svg";
    private readonly VisualIdentityStorageOptions settings = options.Value;
    private readonly string storageRoot = ResolveStorageRoot(environment, options.Value.Directory);

    public long MaximumFileSizeBytes => settings.MaximumFileSizeBytes;

    public async Task<VisualIdentityAssetUploadResult> UploadAsync(
        VisualIdentityAssetKind kind,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedType = GetExpectedContentType(kind, extension);
        if (expectedType is null || !string.Equals(expectedType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return new(VisualIdentityAssetError.UnsupportedType);
        }

        if (fileSize <= 0 || fileSize > settings.MaximumFileSizeBytes)
        {
            return new(VisualIdentityAssetError.TooLarge);
        }

        var entityDirectory = GetEntityDirectory();
        System.IO.Directory.CreateDirectory(entityDirectory);
        var key = $"VisualIdentity/{StorageDirectoryId:N}/{Guid.NewGuid():N}{extension}";
        var destination = ResolvePath(key);
        var temporary = Path.Combine(entityDirectory, $"{Guid.NewGuid():N}.upload");
        var supportsAutomaticRecoloring = false;
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
                        return new(VisualIdentityAssetError.TooLarge);
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (copied != fileSize)
                {
                    return new(VisualIdentityAssetError.InvalidContent);
                }
            }

            if (extension == ".svg")
            {
                await using var svgStream = new FileStream(temporary, FileMode.Open, FileAccess.Read, FileShare.Read);
                var document = await SafeSvgDocument.LoadValidatedAsync(svgStream, cancellationToken);
                if (document is null)
                {
                    return new(VisualIdentityAssetError.InvalidContent);
                }

                supportsAutomaticRecoloring = kind == VisualIdentityAssetKind.MainLogo
                    && VisualIdentitySvgTransformer.SupportsAutomaticRecoloring(document);
            }
            else if (!await HasValidPngContentAsync(temporary, cancellationToken))
            {
                return new(VisualIdentityAssetError.InvalidContent);
            }

            File.Move(temporary, destination);
            return new(VisualIdentityAssetError.None, key, expectedType, fileSize, supportsAutomaticRecoloring);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public Task DeleteAsync(string? storageKey)
    {
        if (storageKey is null || !TryResolvePath(storageKey, out var path))
        {
            return Task.CompletedTask;
        }

        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            var directory = Path.GetDirectoryName(path)!;
            if (System.IO.Directory.Exists(directory) && !System.IO.Directory.EnumerateFileSystemEntries(directory).Any())
            {
                System.IO.Directory.Delete(directory);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Unable to delete visual identity asset {StorageKey}.", storageKey);
        }

        return Task.CompletedTask;
    }

    public Task<VisualIdentityAssetContent?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!TryResolvePath(storageKey, out var path) || !File.Exists(path))
        {
            return Task.FromResult<VisualIdentityAssetContent?>(null);
        }

        var contentType = GetContentType(Path.GetExtension(path));
        if (contentType is null)
        {
            return Task.FromResult<VisualIdentityAssetContent?>(null);
        }

        return Task.FromResult<VisualIdentityAssetContent?>(new VisualIdentityAssetContent(
            new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read),
            contentType));
    }

    public Task<VisualIdentityAssetContent> OpenDefaultLogoAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OpenEmbeddedResource(MainLogoResource, "image/svg+xml"));

    public Task<VisualIdentityAssetContent> OpenDefaultFaviconAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(OpenEmbeddedResource(DefaultFaviconResource, "image/svg+xml"));

    private static VisualIdentityAssetContent OpenEmbeddedResource(string resourceName, string contentType)
    {
        var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"The embedded visual identity asset '{resourceName}' was not found.");
        return new VisualIdentityAssetContent(stream, contentType);
    }

    private string GetEntityDirectory() => Path.Combine(storageRoot, "VisualIdentity", StorageDirectoryId.ToString("N"));

    private static string? GetExpectedContentType(VisualIdentityAssetKind kind, string extension) => (kind, extension) switch
    {
        (VisualIdentityAssetKind.MainLogo or VisualIdentityAssetKind.FooterLogo, ".svg") => "image/svg+xml",
        (VisualIdentityAssetKind.Favicon, ".svg") => "image/svg+xml",
        (VisualIdentityAssetKind.Favicon, ".png") => "image/png",
        _ => null
    };

    private static string? GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".svg" => "image/svg+xml",
        ".png" => "image/png",
        _ => null
    };

    private static async Task<bool> HasValidPngContentAsync(string path, CancellationToken cancellationToken)
    {
        var header = new byte[8];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var read = await stream.ReadAsync(header, cancellationToken);
        return read == 8 && header.SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
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
            throw new InvalidOperationException("Visual identity asset storage must be outside the web root.");
        }

        System.IO.Directory.CreateDirectory(root);
        return root;
    }

    private string ResolvePath(string key)
    {
        if (!TryResolvePath(key, out var path))
        {
            throw new InvalidOperationException("The visual identity asset storage key is invalid.");
        }

        return path;
    }

    private bool TryResolvePath(string key, out string path)
    {
        path = string.Empty;
        var prefix = $"VisualIdentity/{StorageDirectoryId:N}/";
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
            || extension is not (".svg" or ".png"))
        {
            return false;
        }

        var entityRoot = Path.GetFullPath(GetEntityDirectory());
        path = Path.GetFullPath(Path.Combine(entityRoot, fileName));
        return path.StartsWith(entityRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }
}
