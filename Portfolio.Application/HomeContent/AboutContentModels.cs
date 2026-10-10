namespace Portfolio.Application.HomeContent;

public sealed record AboutContentEditRequest(
    string SpanishProfile,
    string? EnglishProfile,
    IReadOnlyList<SkillGroupEditModel> Groups,
    IReadOnlyList<HobbyEditModel> Hobbies);

public sealed record SkillGroupEditModel(
    Guid Id,
    string SpanishName,
    string? EnglishName,
    int DisplayOrder,
    IReadOnlyList<SkillEditModel> Skills);

public sealed record SkillEditModel(Guid Id, string SpanishName, string? EnglishName, int DisplayOrder);

public sealed record HobbyEditModel(
    Guid Id,
    string StorageKey,
    string ContentType,
    long SizeBytes,
    int DisplayOrder,
    string SpanishName,
    string SpanishDescription,
    string? EnglishName,
    string? EnglishDescription);

public sealed record AboutValidationError(string Field, string ResourceKey);

public sealed record AboutSaveResult(bool Succeeded, IReadOnlyList<AboutValidationError> Errors);

public sealed record AboutContentReadModel(
    string? Profile,
    IReadOnlyList<SkillGroupReadModel> Groups,
    IReadOnlyList<HobbyReadModel> Hobbies);

public sealed record SkillGroupReadModel(string Name, int DisplayOrder, IReadOnlyList<SkillReadModel> Skills);

public sealed record SkillReadModel(string Name, int DisplayOrder);

public sealed record HobbyReadModel(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl,
    int DisplayOrder);