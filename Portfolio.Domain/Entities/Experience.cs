namespace Portfolio.Domain.Entities;

public sealed class Experience
{
    public Guid Id { get; set; }

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int DisplayOrder { get; set; }

    public ICollection<ExperienceTranslation> Translations { get; set; } = [];
}
