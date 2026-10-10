namespace Portfolio.Domain.Entities;

public sealed class HobbyTranslation
{
    public Guid HobbyId { get; set; }

    public string LanguageCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public Hobby Hobby { get; set; } = null!;
}