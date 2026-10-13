namespace Portfolio.Application.VisualIdentity;

public interface IVisualIdentityAdministrationService
{
    Task<VisualIdentitySettingsModel> GetAsync(CancellationToken cancellationToken = default);

    Task<VisualIdentityAssetContent?> OpenPreviewAssetAsync(
        VisualIdentityAssetKind kind,
        VisualIdentityEditRequest request,
        CancellationToken cancellationToken = default);

    Task<VisualIdentitySaveResult> SaveAsync(
        VisualIdentityEditRequest request,
        CancellationToken cancellationToken = default);

    Task<VisualIdentityAssetUploadResult> UploadAssetAsync(
        VisualIdentityAssetKind kind,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task RemoveManualVariantAsync(
        VisualIdentityAssetKind kind,
        CancellationToken cancellationToken = default);

    Task<VisualIdentitySaveResult> RestoreDefaultsAsync(CancellationToken cancellationToken = default);
}
