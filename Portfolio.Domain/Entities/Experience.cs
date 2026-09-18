namespace Portfolio.Domain.Entities;

public sealed class Experience
{
    public Guid Id { get; set; }

    public string RoleTitle { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;

    public string Summary { get; set; } = string.Empty;

    public DateOnly StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public int DisplayOrder { get; set; }
}
