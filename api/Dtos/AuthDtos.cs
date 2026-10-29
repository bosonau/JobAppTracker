using System.ComponentModel.DataAnnotations;
namespace JobTrackerApi.Dtos;
public record LoginRequest([Required] string Username, [Required] string Password);
public record LoginResponse(string Token);