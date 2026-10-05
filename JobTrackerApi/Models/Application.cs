namespace JobTrackerApi.Models;

public class Application
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string RoleTitle { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateOnly DateApplied { get; set; }
    public string? SourceUrl { get; set; }
    public string? Notes { get; set; }
}

public enum ApplicationStatus
{
    Applied,
    Screening,
    Interview,
    Offer,
    Rejected,
    Withdrawn
}