using System.Security.Claims;
using System.Text;
using JobTrackerApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace JobTrackerApi.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config) => _config = config;

    [HttpPost("login")]
    public ActionResult<LoginResponse> Login(LoginRequest request)
    {
        // Demo user, credentials come from config
        var demoUser = _config["DemoUser:Username"];
        var demoPassword = _config["DemoUser:Password"];
        if (string.IsNullOrEmpty(demoUser) || string.IsNullOrEmpty(demoPassword))
        {
            return Problem("Demo user is not configured.");
        }        
        
        if (!SecretEquals(request.Username, demoUser) || !SecretEquals(request.Password, demoPassword))
        {
            return Unauthorized();
        }
            

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Subject = new ClaimsIdentity(new[] {new Claim(ClaimTypes.Name, request.Username)}),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new LoginResponse(token);
    }

    private static bool SecretEquals(string a, string b) =>
        CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(a)),
            SHA256.HashData(Encoding.UTF8.GetBytes(b))
        );
}