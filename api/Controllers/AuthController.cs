using System.Security.Claims;
using System.Text;
using JobTrackerApi.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

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
        // Demo, a hard-coded user
        // TODO: set up either an identity provider or a hashed lookup in a user store
        if (request.Username != "demo" || request.Password != "demo-password")
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
}