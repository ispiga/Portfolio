namespace Portfolio.Domain.Entities;

public sealed class Skill
{
    public Guid Id { get; set; }

    public Guid SkillGroupId { get; set; }

    public int DisplayOrder { get; set; }

    public SkillGroup SkillGroup { get; set; } = null!;

    public ICollection<SkillTranslation> Translations { get; set; } = [];
}