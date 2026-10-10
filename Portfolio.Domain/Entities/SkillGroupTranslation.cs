namespace Portfolio.Domain.Entities;

public sealed class SkillGroupTranslation
{
    public Guid SkillGroupId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public SkillGroup SkillGroup { get; set; } = null!;
}