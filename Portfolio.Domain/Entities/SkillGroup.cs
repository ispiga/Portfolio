namespace Portfolio.Domain.Entities;

public sealed class SkillGroup
{
    public Guid Id { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<SkillGroupTranslation> Translations { get; set; } = [];

    public ICollection<Skill> Skills { get; set; } = [];
}