namespace Portfolio.Application.VisualIdentity;

public interface IVisualIdentityAssetStorageService
{
    long MaximumFileSizeBytes { get; }

    Task<VisualIdentityAssetUploadResult> UploadAsync(
        VisualIdentityAssetKind kind,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(string? storageKey);

    Task<VisualIdentityAssetContent?> OpenReadAsync(
        string storageKey,
        CancellationToken cancellationToken = default);

    Task<VisualIdentityAssetContent> OpenDefaultLogoAsync(CancellationToken cancellationToken = default);

    Task<VisualIdentityAssetContent> OpenDefaultFaviconAsync(CancellationToken cancellationToken = default);
}
