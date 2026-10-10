namespace Portfolio.Application.HomeContent;

public sealed record HeroEditRequest(
    int OrbitCount,
    string SpanishHeadline,
    string? EnglishHeadline,
    IReadOnlyList<TechnologyLogoEditModel> Logos,
    Guid? ProfileImageId = null,
    string? ProfileImageStorageKey = null,
    string? ProfileImageContentType = null,
    long? ProfileImageSizeBytes = null,
    string? ProfileImageAlternativeText = null);

public sealed record TechnologyLogoEditModel(
    Guid Id,
    string Name,
    string AlternativeText,
    string StorageKey,
    string ContentType,
    long SizeBytes,
    int DisplayOrder,
    int Orbit);

public sealed record HeroValidationError(string Field, string ResourceKey);

public sealed record HeroSaveResult(bool Succeeded, IReadOnlyList<HeroValidationError> Errors);

public sealed record HeroReadModel(
    string Headline,
    int OrbitCount,
    IReadOnlyList<TechnologyLogoReadModel> Logos,
    string? ProfileImageUrl = null,
    string? ProfileImageAlternativeText = null);

public sealed record TechnologyLogoReadModel(
    Guid Id,
    string Name,
    string AlternativeText,
    string ImageUrl,
    int DisplayOrder,
    int Orbit);