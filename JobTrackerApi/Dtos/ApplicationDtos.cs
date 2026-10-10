using JobTrackerApi.Models;
using System.ComponentModel.DataAnnotations;

namespace JobTrackerApi.Dtos;

//create an application
public record CreateApplicationRequest {
    [Required, MaxLength(250)]
    public string CompanyName {get; init;} = string.Empty;
    [Required, MaxLength(250)]
    public string RoleTitle {get; init;} = string.Empty;
    [EnumDataType(typeof(ApplicationStatus))]
    public ApplicationStatus Status {get; init;}
    [Required]
    public DateOnly? DateApplied {get; init;}
    [Url, MaxLength(2048)]
    public string? SourceUrl {get; init;}
    [MaxLength(4000)]
    public string? Notes {get; init;}
};
//update an application --note its the same as create for now--
public record UpdateApplicationRequest {
    [Required, MaxLength(250)]
    public string CompanyName {get; init;} = string.Empty;
    [Required, MaxLength(250)]
    public string RoleTitle {get; init;} = string.Empty;
    [EnumDataType(typeof(ApplicationStatus))]
    public ApplicationStatus Status {get; init;}
    [Required]
    public DateOnly? DateApplied {get; init;}
    [Url, MaxLength(2048)]
    public string? SourceUrl {get; init;}
    [MaxLength(4000)]
    public string? Notes {get; init;}
};
//what the API returns
public record ApplicationResponse (
    int Id,
    string CompanyName,
    string RoleTitle,
    ApplicationStatus Status,
    DateOnly DateApplied,
    string? SourceUrl,
    string? Notes
);

public static class ApplicationMappings
{
    public static ApplicationResponse ToResponse(this Application a) =>
        new(a.Id, a.CompanyName, a.RoleTitle, a.Status, a.DateApplied, a.SourceUrl, a.Notes);

    public static Application ToEntity(this CreateApplicationRequest r) => new()
    {
        CompanyName = r.CompanyName,
        RoleTitle = r.RoleTitle,
        Status = r.Status,
        DateApplied = r.DateApplied!.Value,
        SourceUrl = r.SourceUrl,
        Notes = r.Notes
    };

    public static void ApplyTo(this UpdateApplicationRequest r, Application a)
    {
        a.CompanyName = r.CompanyName;
        a.RoleTitle = r.RoleTitle;
        a.Status = r.Status;
        a.DateApplied = r.DateApplied!.Value;
        a.SourceUrl = r.SourceUrl;
        a.Notes = r.Notes;
    }
}