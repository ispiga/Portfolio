using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Portfolio.Application.Projects;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Storage;

public sealed class ProjectPreviewImageStorageService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    IOptions<ProjectPreviewImageStorageOptions> options) : IProjectPreviewImageService
{
    private const string PublicPathPrefix = "/project-preview-images/";
    private readonly ProjectPreviewImageStorageOptions settings = options.Value;
    private readonly string storageRoot = ResolveStorageRoot(environment, options.Value.Directory);

    public long MaximumFileSizeBytes => settings.MaximumFileSizeBytes;

    public async Task<ProjectImageOperationResult> UploadAsync(
        Guid projectId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedContentType = GetContentType(extension);
        if (expectedContentType is null || !string.Equals(expectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return new(ProjectImageError.UnsupportedType);
        }

        if (fileSize <= 0 || fileSize > settings.MaximumFileSizeBytes)
        {
            return new(ProjectImageError.TooLarge);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var project = await context.Projects.SingleOrDefaultAsync(candidate => candidate.Id == projectId, cancellationToken);
        if (project is null)
        {
            return new(ProjectImageError.ProjectNotFound);
        }

        var fileId = Guid.NewGuid().ToString("N");
        var destination = Path.Combine(storageRoot, $"{fileId}{extension}");
        var temporary = Path.Combine(storageRoot, $"{Guid.NewGuid():N}.upload");
        Directory.CreateDirectory(storageRoot);
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
                        return new(ProjectImageError.TooLarge);
                    }

                    await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }

                if (copied != fileSize)
                {
                    return new(ProjectImageError.InvalidContent);
                }
            }

            if (!await HasValidSignatureAsync(temporary, extension, cancellationToken))
            {
                return new(ProjectImageError.InvalidContent);
            }

            File.Move(temporary, destination);
            var oldPath = project.PreviewImagePath;
            project.PreviewImagePath = PublicPathPrefix + Path.GetFileName(destination);
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                File.Delete(destination);
                throw;
            }

            DeleteStoredPath(oldPath);
            return new(ProjectImageError.None, project.PreviewImagePath);
        }
        finally
        {
            if (File.Exists(temporary))
            {
                File.Delete(temporary);
            }
        }
    }

    public async Task<bool> DeleteAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var project = await context.Projects.SingleOrDefaultAsync(candidate => candidate.Id == projectId, cancellationToken);
        if (project is null)
        {
            return false;
        }

        var oldPath = project.PreviewImagePath;
        project.PreviewImagePath = null;
        await context.SaveChangesAsync(cancellationToken);
        DeleteStoredPath(oldPath);
        return true;
    }

    public async Task<ProjectImageContent?> OpenPublicReadAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var path = await context.Projects.AsNoTracking()
            .Where(project => project.Id == projectId)
            .Select(project => project.PreviewImagePath)
            .SingleOrDefaultAsync(cancellationToken);
        if (path is null || !TryGetStoredPath(path, out var fullPath) || !File.Exists(fullPath))
        {
            return null;
        }

        var contentType = Path.GetExtension(fullPath).ToLowerInvariant() == ".png" ? "image/png" : "image/jpeg";
        return new ProjectImageContent(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read), contentType);
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
            throw new InvalidOperationException("Project preview image storage must be outside the web root.");
        }

        Directory.CreateDirectory(root);
        return root;
    }

    private static string? GetContentType(string extension) => extension switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => null
    };

    private static async Task<bool> HasValidSignatureAsync(
        string path,
        string extension,
        CancellationToken cancellationToken)
    {
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

    private bool TryGetStoredPath(string publicPath, out string fullPath)
    {
        fullPath = string.Empty;
        if (!publicPath.StartsWith(PublicPathPrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var fileName = publicPath[PublicPathPrefix.Length..];
        if (fileName.Length is < 32 or > 40
            || fileName.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '.'))
        {
            return false;
        }

        fullPath = Path.GetFullPath(Path.Combine(storageRoot, fileName));
        return fullPath.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
    }

    private void DeleteStoredPath(string? publicPath)
    {
        if (publicPath is not null && TryGetStoredPath(publicPath, out var fullPath) && File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}
