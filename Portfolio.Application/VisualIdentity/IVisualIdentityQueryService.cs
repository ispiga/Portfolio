namespace Portfolio.Application.VisualIdentity;

public interface IVisualIdentityQueryService
{
    Task<VisualIdentitySettingsModel> GetAsync(CancellationToken cancellationToken = default);

    Task<VisualIdentityAssetContent?> OpenAssetAsync(
        VisualIdentityAssetKind kind,
        Guid assetVersion,
        CancellationToken cancellationToken = default);
}
