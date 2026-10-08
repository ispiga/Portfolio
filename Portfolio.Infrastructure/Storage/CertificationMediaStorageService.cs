using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Portfolio.Application.Certifications;
using Portfolio.Domain.Entities;

namespace Portfolio.Infrastructure.Storage;

public sealed class CertificationMediaStorageService(
    IDbContextFactory<PortfolioDbContext> dbContextFactory,
    IWebHostEnvironment environment,
    IOptions<CertificationMediaStorageOptions> options,
    ILogger<CertificationMediaStorageService> logger) : ICertificationMediaService
{
    private const string StoragePrefix = "certifications";
    private readonly CertificationMediaStorageOptions settings = options.Value;
    private readonly string storageRoot = ResolveStorageRoot(environment, options.Value.Directory);

    public long MaximumFileSizeBytes => settings.MaximumFileSizeBytes;

    public async Task<IReadOnlyList<CertificationAttachmentReadModel>> GetAttachmentsAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachments = await context.CertificationAttachments
            .AsNoTracking()
            .Where(attachment => attachment.CertificationId == certificationId)
            .OrderBy(attachment => attachment.CreatedAt)
            .ThenBy(attachment => attachment.Id)
            .ToListAsync(cancellationToken);
        return attachments.Select(ToReadModel).ToArray();
    }

    public async Task<CertificationAttachmentOperationResult> UploadAttachmentAsync(
        Guid certificationId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateFile(fileName, contentType, fileSize, allowPdf: true);
        if (validation.Error is { } error)
        {
            return new(error);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        if (!await context.Certifications.AnyAsync(certification => certification.Id == certificationId, cancellationToken))
        {
            return new(CertificationAttachmentError.CertificationNotFound);
        }

        var safeFileName = GetSafeFileName(fileName);
        if (safeFileName is null)
        {
            return new(CertificationAttachmentError.UnsupportedType);
        }

        var id = Guid.NewGuid();
        var relativePath = $"{StoragePrefix}/{certificationId:N}/attachments/{id:N}{validation.Extension}";
        var destination = ResolveStoragePath(relativePath);
        var temporaryPath = Path.Combine(storageRoot, $"{Guid.NewGuid():N}.upload");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        try
        {
            var copyResult = await CopyAndValidateAsync(content, temporaryPath, fileSize, validation.Extension!, cancellationToken);
            if (copyResult is { } copyError)
            {
                return new(copyError);
            }

            File.Move(temporaryPath, destination);
            var attachment = new CertificationAttachment
            {
                Id = id,
                CertificationId = certificationId,
                OriginalFileName = safeFileName,
                DisplayName = safeFileName,
                StorageKey = relativePath,
                ContentType = validation.ContentType!,
                SizeBytes = fileSize,
                CreatedAt = DateTimeOffset.UtcNow,
                IsPublic = false
            };
            context.CertificationAttachments.Add(attachment);
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                File.Delete(destination);
                throw;
            }

            return new(CertificationAttachmentError.None, ToReadModel(attachment));
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public async Task<bool> UpdateAttachmentDisplayNameAsync(
        Guid attachmentId,
        string displayName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return false;
        }

        var safeDisplayName = displayName.Trim();
        if (safeDisplayName.Length > 255)
        {
            return false;
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.CertificationAttachments.SingleOrDefaultAsync(
            candidate => candidate.Id == attachmentId,
            cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        attachment.DisplayName = safeDisplayName;
        await context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> SetAttachmentPublicAsync(
        Guid attachmentId,
        bool isPublic,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.CertificationAttachments.SingleOrDefaultAsync(
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

    public async Task<bool> DeleteAttachmentAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.CertificationAttachments
            .SingleOrDefaultAsync(candidate => candidate.Id == attachmentId, cancellationToken);
        if (attachment is null)
        {
            return false;
        }

        var storedPath = TryGetStoredPath(attachment.StorageKey, attachment.CertificationId, "attachments", out var path)
            ? path
            : null;
        context.CertificationAttachments.Remove(attachment);
        await context.SaveChangesAsync(cancellationToken);
        DeleteFile(storedPath);
        return true;
    }

    public async Task<CertificationAttachmentContent?> OpenAttachmentAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.CertificationAttachments.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == attachmentId, cancellationToken);
        if (attachment is null
            || !TryGetStoredPath(attachment.StorageKey, attachment.CertificationId, "attachments", out var path)
            || !File.Exists(path))
        {
            return null;
        }

        return new(ToReadModel(attachment), new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public async Task<CertificationAttachmentContent?> OpenPublicAttachmentAsync(
        Guid attachmentId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var attachment = await context.CertificationAttachments.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == attachmentId && candidate.IsPublic, cancellationToken);
        if (attachment is null
            || !TryGetStoredPath(attachment.StorageKey, attachment.CertificationId, "attachments", out var path)
            || !File.Exists(path))
        {
            return null;
        }

        return new(ToReadModel(attachment), new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public async Task<CertificationCardImageOperationResult> UploadCardImageAsync(
        Guid certificationId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default)
    {
        var validation = ValidateFile(fileName, contentType, fileSize, allowPdf: false);
        if (validation.Error is { } error)
        {
            return new(error);
        }

        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var certification = await context.Certifications
            .SingleOrDefaultAsync(candidate => candidate.Id == certificationId, cancellationToken);
        if (certification is null)
        {
            return new(CertificationAttachmentError.CertificationNotFound);
        }

        var relativePath = $"{StoragePrefix}/{certificationId:N}/card/{Guid.NewGuid():N}{validation.Extension}";
        var destination = ResolveStoragePath(relativePath);
        var temporaryPath = Path.Combine(storageRoot, $"{Guid.NewGuid():N}.upload");
        Directory.CreateDirectory(Path.GetDirectoryName(destination)!);

        try
        {
            var copyResult = await CopyAndValidateAsync(content, temporaryPath, fileSize, validation.Extension!, cancellationToken);
            if (copyResult is { } copyError)
            {
                return new(copyError);
            }

            File.Move(temporaryPath, destination);
            var oldPath = certification.ImagePath;
            certification.ImagePath = relativePath;
            try
            {
                await context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                File.Delete(destination);
                throw;
            }

            DeleteStoredPath(oldPath, certificationId, "card");
            return new(CertificationAttachmentError.None);
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    public async Task<bool> DeleteCardImageAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var certification = await context.Certifications
            .SingleOrDefaultAsync(candidate => candidate.Id == certificationId, cancellationToken);
        if (certification is null)
        {
            return false;
        }

        var oldPath = certification.ImagePath;
        certification.ImagePath = null;
        await context.SaveChangesAsync(cancellationToken);
        DeleteStoredPath(oldPath, certificationId, "card");
        return true;
    }

    public async Task<CertificationImageContent?> OpenCardImageAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var imagePath = await context.Certifications.AsNoTracking()
            .Where(certification => certification.Id == certificationId)
            .Select(certification => certification.ImagePath)
            .SingleOrDefaultAsync(cancellationToken);
        if (imagePath is null
            || !TryGetStoredPath(imagePath, certificationId, "card", out var path)
            || !File.Exists(path))
        {
            return null;
        }

        var content = await File.ReadAllBytesAsync(path, cancellationToken);
        return new(GetContentType(Path.GetExtension(path))!, new MemoryStream(content, writable: false));
    }

    public async Task DeleteCertificationFilesAsync(
        Guid certificationId,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var certification = await context.Certifications
            .Include(candidate => candidate.Attachments)
            .SingleOrDefaultAsync(candidate => candidate.Id == certificationId, cancellationToken);
        if (certification is null)
        {
            return;
        }

        var files = certification.Attachments
            .Select(attachment => TryGetStoredPath(attachment.StorageKey, certificationId, "attachments", out var path) ? path : null)
            .Append(TryGetStoredPath(certification.ImagePath, certificationId, "card", out var imagePath) ? imagePath : null)
            .ToArray();
        context.CertificationAttachments.RemoveRange(certification.Attachments);
        certification.ImagePath = null;
        await context.SaveChangesAsync(cancellationToken);
        foreach (var path in files)
        {
            DeleteFile(path);
        }
    }

    public Task CleanupCertificationDirectoryAsync(Guid certificationId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CleanupCertificationDirectory(certificationId);
        return Task.CompletedTask;
    }

    private void CleanupCertificationDirectory(Guid certificationId)
    {
        var storagePrefixDirectory = Path.GetFullPath(Path.Combine(storageRoot, StoragePrefix));
        var certificationDirectory = Path.GetFullPath(Path.Combine(storagePrefixDirectory, certificationId.ToString("N")));
        if (!certificationDirectory.StartsWith(storagePrefixDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Skipped cleanup of an invalid media directory for certification {CertificationId}.", certificationId);
            return;
        }

        TryDeleteEmptyDirectory(Path.Combine(certificationDirectory, "attachments"), certificationId);
        TryDeleteEmptyDirectory(Path.Combine(certificationDirectory, "card"), certificationId);
        TryDeleteEmptyDirectory(certificationDirectory, certificationId);
    }

    private async Task<CertificationAttachmentError?> CopyAndValidateAsync(
        Stream content,
        string temporaryPath,
        long expectedSize,
        string extension,
        CancellationToken cancellationToken)
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
            long copied = 0;
            int read;
            while ((read = await content.ReadAsync(buffer, cancellationToken)) > 0)
            {
                copied += read;
                if (copied > settings.MaximumFileSizeBytes)
                {
                    return CertificationAttachmentError.TooLarge;
                }

                await output.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
            }

            if (copied != expectedSize)
            {
                return CertificationAttachmentError.InvalidContent;
            }
        }

        return await HasValidSignatureAsync(temporaryPath, extension, cancellationToken)
            ? null
            : CertificationAttachmentError.InvalidContent;
    }

    private (CertificationAttachmentError? Error, string? Extension, string? ContentType) ValidateFile(
        string fileName,
        string contentType,
        long fileSize,
        bool allowPdf)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!settings.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase)
            || (!allowPdf && extension == ".pdf")
            || GetContentType(extension) is not { } expectedContentType
            || !string.Equals(expectedContentType, contentType, StringComparison.OrdinalIgnoreCase))
        {
            return (CertificationAttachmentError.UnsupportedType, null, null);
        }

        if (fileSize <= 0 || fileSize > settings.MaximumFileSizeBytes)
        {
            return (CertificationAttachmentError.TooLarge, null, null);
        }

        return (null, extension, expectedContentType);
    }

    private static string? GetSafeFileName(string fileName)
    {
        var safeFileName = Path.GetFileName(fileName.Replace('\\', '/'));
        return string.IsNullOrWhiteSpace(safeFileName) || safeFileName.Length > 255 ? null : safeFileName;
    }

    private static CertificationAttachmentReadModel ToReadModel(CertificationAttachment attachment) => new(
        attachment.Id,
        attachment.CertificationId,
        attachment.OriginalFileName,
        attachment.DisplayName,
        attachment.ContentType,
        attachment.SizeBytes,
        attachment.CreatedAt,
        attachment.IsPublic);

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
            throw new InvalidOperationException("Certification media storage must be outside the web root.");
        }

        Directory.CreateDirectory(root);
        return root;
    }

    private string ResolveStoragePath(string relativePath)
    {
        var fullPath = Path.GetFullPath(Path.Combine(storageRoot, relativePath.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(storageRoot + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Certification media path is outside the configured storage directory.");
        }

        return fullPath;
    }

    private bool TryGetStoredPath(string? storageKey, Guid certificationId, string category, out string fullPath)
    {
        fullPath = string.Empty;
        var expectedPrefix = $"{StoragePrefix}/{certificationId:N}/{category}/";
        if (string.IsNullOrWhiteSpace(storageKey)
            || !storageKey.StartsWith(expectedPrefix, StringComparison.Ordinal)
            || storageKey.Length > expectedPrefix.Length + 40
            || storageKey[expectedPrefix.Length..].Any(character => !char.IsAsciiLetterOrDigit(character) && character != '.'))
        {
            return false;
        }

        fullPath = ResolveStoragePath(storageKey);
        return true;
    }

    private void DeleteStoredPath(string? storageKey, Guid certificationId, string category)
    {
        if (TryGetStoredPath(storageKey, certificationId, category, out var path))
        {
            DeleteFile(path);
        }
    }

    private void DeleteFile(string? path)
    {
        try
        {
            if (path is not null && File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Unable to delete certification media file {FilePath}.", path);
        }
    }

    private void TryDeleteEmptyDirectory(string path, Guid certificationId)
    {
        try
        {
            if (Directory.Exists(path) && !Directory.EnumerateFileSystemEntries(path).Any())
            {
                Directory.Delete(path);
            }
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            logger.LogWarning(exception, "Unable to delete empty media directory {DirectoryPath} for certification {CertificationId}.", path, certificationId);
        }
    }

    private static string? GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".pdf" => "application/pdf",
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
            ".pdf" => read >= 5 && header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D,
            _ => false
        };
    }
}
