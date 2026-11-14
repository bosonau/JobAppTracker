public class ApplicationDoc
{
    public int Id { get; set;}
    public required string UserId {get; set;}
    public required string Company {get; set;}
    public required string Title {get; set;}
    public required string Status {get; set;}
    public string? Notes {get; set;}
    public DateOnly? AppliedDate {get; set;}
}