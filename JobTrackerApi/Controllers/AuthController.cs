using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

public record LoginRequest(string Username, string Password);

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]

public class AuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public AuthController(IConfiguration config) => _config = config;

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        //Demo, a hard-coded user
        //todo: set up either an identity provider or a hashed lookup in a user store
        if (request.Username !="demo" || request.Password !="demo-password")
            return Unauthorized();

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt.key"]!));
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            Subject = new System.Security.Claims.ClaimsIdentity(new[] {new Claim(ClaimTypes.Name, request.Username)}),
            Expires = DateTime.UtcNow.AddMinutes(30),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return Ok(new { token });
    }    
}