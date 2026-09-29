using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Experiences;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Storage;

public sealed class ExperienceAttachmentStorageService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    IOptions<ExperienceAttachmentStorageOptions> options,
    ILogger<ExperienceAttachmentStorageService> logger) : IExperienceAttachmentService
{
    private readonly ExperienceAttachmentStorageOptions settings = options.Value;
    private readonly string storageRoot = GetStorageRoot(environment, options.Value.Directory);

    public int MaximumFileCount => settings.MaximumFileCount;

    public long MaximumFileSizeBytes => settings.MaximumFileSizeBytes;

    public async Task<IReadOnlyList<ExperienceAttachmentReadModel>> GetAttachmentsAsync(
        Guid experienceId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachments = await context.ExperienceAttachments
            .AsNoTracking()
            .Where(attachment => attachment.ExperienceId == experienceId)
            .OrderBy(attachment => attachment.CreatedAt)
            .ThenBy(attachment => attachment.Id)
            .ToListAsync(cancellationToken);
        return attachments.Select(ToReadModel).ToArray();
    }

    public async Task<ExperienceAttachmentOperationResult> UploadAsync(
        Guid experienceId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        var expectedContentType = GetContentType(extension);
        if (!settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
            || expectedContentType is null
            || !string.Equals(expectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return new(ExperienceAttachmentError.UnsupportedType);
        }

        if (fileSize <= 0 || fileSize > settings.MaximumFileSizeBytes)
        {
            return new(ExperienceAttachmentError.TooLarge);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await context.Experiences.AnyAsync(experience => experience.Id == experienceId, cancellationToken))
        {
            return new(ExperienceAttachmentError.ExperienceNotFound);
        }

        if (await context.ExperienceAttachments.CountAsync(
                attachment => attachment.ExperienceId == experienceId,
                cancellationToken) >= settings.MaximumFileCount)
        {
            return new(ExperienceAttachmentError.TooManyFiles);
        }

        var safeFileName = Path.GetFileName(fileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName.Length > 255)
        {
            return new(ExperienceAttachmentError.UnsupportedType);
        }

        var id = Guid.NewGuid();
        var relativeStorageKey = Path.Combine("experience", experienceId.ToString("N"), $"{id:N}{extension}");
        var destination = ResolveStoragePath(relativeStorageKey);
        var temporaryPath = Path.Combine(storageRoot, $"{Guid.NewGuid():N}.upload");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        try
        {
            await using (var output = new FileStream(
                temporaryPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                var buffer = new byte[81920];
                long bytesCopied = 0;
                int bytesRead;
                while ((bytesRead = await content.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    bytesCopied += bytesRead;
                    if (bytesCopied > settings.MaximumFileSizeBytes)
                    {
                        return new(ExperienceAttachmentError.TooLarge);
                    }

                    await output.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                }

                if (bytesCopied != fileSize)
                {
                    return new(ExperienceAttachmentError.InvalidContent);
                }
            }

            if (!await HasValidSignatureAsync(temporaryPath, extension, cancellationToken))
            {
                return new(ExperienceAttachmentError.InvalidContent);
            }

            File.Move(temporaryPath, destination);
            var attachment = new ExperienceAttachment
            {
                Id = id,
                ExperienceId = experienceId,
                OriginalFileName = safeFileName,
                StorageKey = relativeStorageKey.Replace('\\', '/'),
                ContentType = expectedContentType,
                SizeBytes = fileSize,
                CreatedAt = DateTimeOffset.UtcNow,
                IsPublic = false
            };
            context.ExperienceAttachments.Add(attachment);
            await context.SaveChangesAsync(cancellationToken);
            return new(ExperienceAttachmentError.None, ToReadModel(attachment));
        }
        catch
        {
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            throw;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public async Task<bool> DeleteAsync(Guid attachmentId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.ExperienceAttachments.SingleOrDefaultAsync(
            candidate => candidate.Id == attachmentId,
            cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        context.ExperienceAttachments.Remove(attachment);
        await context.SaveChangesAsync(cancellationToken);
        try
        {
            var path = ResolveStoragePath(attachment.StorageKey);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException exception)
        {
            logger.LogWarning(exception, "Unable to remove a deleted experience attachment from persistent storage.");
        }

        return true;
    }

    public async Task<bool> SetPublicAsync(Guid attachmentId, bool isPublic, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.ExperienceAttachments.SingleOrDefaultAsync(
            candidate => candidate.Id == attachmentId,
            cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        attachment.IsPublic = isPublic;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ExperienceAttachmentContent?> OpenReadAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.ExperienceAttachments.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == attachmentId, cancellationToken);
        if (attachment is null)
        {
            return null;
        }

        var path = ResolveStoragePath(attachment.StorageKey);
        if (!File.Exists(path))
        {
            return null;
        }

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return new(ToReadModel(attachment), stream);
    }

    public async Task<ExperienceAttachmentContent?> OpenPublicReadAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.ExperienceAttachments.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == attachmentId && candidate.IsPublic, cancellationToken);
        if (attachment is null)
        {
            return null;
        }

        var path = ResolveStoragePath(attachment.StorageKey);
        if (!File.Exists(path))
        {
            return null;
        }

        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return new(ToReadModel(attachment), stream);
    }

    private string ResolveStoragePath(string storageKey)
    {
        var path = Path.GetFullPath(Path.Combine(storageRoot, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!path.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("The experience attachment storage key is invalid.");
        }

        return path;
    }

    private static string GetStorageRoot(IWebHostEnvironment environment, string configuredDirectory)
    {
        var root = Path.GetFullPath(Path.IsPathRooted(configuredDirectory)
            ? configuredDirectory
            : Path.Combine(environment.ContentRootPath, configuredDirectory));
        var webRoot = Path.GetFullPath(environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
        if (string.Equals(root, webRoot, StringComparison.OrdinalIgnoreCase)
            || root.StartsWith(webRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Experience attachment storage must be outside the web root.");
        }

        Directory.CreateDirectory(root);
        return root;
    }

    private static string? GetContentType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        _ => null
    };

    private static async Task<bool> HasValidSignatureAsync(
        string path,
        string extension,
        CancellationToken cancellationToken)
    {
        var signature = new byte[8];
        await using var input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 8, FileOptions.Asynchronous);
        var bytesRead = await input.ReadAsync(signature, cancellationToken);
        return extension switch
        {
            ".pdf" => bytesRead >= 5 && signature.AsSpan(0, 5).SequenceEqual("%PDF-"u8),
            ".jpg" or ".jpeg" => bytesRead >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF,
            ".png" => bytesRead == 8 && signature.AsSpan().SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            _ => false
        };
    }

    private static ExperienceAttachmentReadModel ToReadModel(ExperienceAttachment attachment) => new(
        attachment.Id,
        attachment.ExperienceId,
        attachment.OriginalFileName,
        attachment.ContentType,
        attachment.SizeBytes,
        attachment.CreatedAt,
        attachment.IsPublic);
}
