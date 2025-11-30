using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManager.Api.Data;
using TaskManager.Api.Models;

namespace TaskManager.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(User user)
    {
        if (await _context.Users.AnyAsync(u => u.Username == user.Username))
            return BadRequest("Username already exists.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Ok("User registered.");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(User user)
    {
        var dbUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username);

        if (dbUser == null || !BCrypt.Net.BCrypt.Verify(user.PasswordHash, dbUser.PasswordHash))
            return Unauthorized("Invalid credentials.");

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, dbUser.Username),
            new Claim("UserId", dbUser.Id.ToString())
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("THIS_IS_A_SUPER_SECRET_KEY_12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(5),
            signingCredentials: creds);

        return Ok(new
        {
            token = new JwtSecurityTokenHandler().WriteToken(token)
        });
    }
}
