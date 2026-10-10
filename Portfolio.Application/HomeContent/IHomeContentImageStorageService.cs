namespace Portfolio.Application.HomeContent;

public enum HomeContentImageKind
{
    TechnologyLogo,
    Hobby,
    HeroProfile
}

public enum HomeContentImageError
{
    None,
    UnsupportedType,
    TooLarge,
    InvalidContent,
    NotFound
}

public sealed record HomeContentImageUploadResult(
    HomeContentImageError Error,
    string? StorageKey = null,
    string? ContentType = null,
    long SizeBytes = 0);

public sealed record HomeContentImageContent(Stream Content, string ContentType);

public interface IHomeContentImageStorageService
{
    long MaximumFileSizeBytes { get; }

    Task<HomeContentImageUploadResult> UploadAsync(
        HomeContentImageKind kind,
        Guid entityId,
        string fileName,
        string contentType,
        long fileSize,
        Stream content,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(HomeContentImageKind kind, Guid entityId, string? storageKey);

    Task<HomeContentImageContent?> OpenPublicReadAsync(
        HomeContentImageKind kind,
        Guid entityId,
        CancellationToken cancellationToken = default);
}