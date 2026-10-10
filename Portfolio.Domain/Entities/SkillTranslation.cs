namespace Portfolio.Domain.Entities;

public sealed class SkillTranslation
{
    public Guid SkillId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public Skill Skill { get; set; } = null!;
}