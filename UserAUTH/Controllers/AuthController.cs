using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserAUTH.Data;
using UserAUTH.Models;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _db;

    public AuthController(AppDbContext db) => _db = db;

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            return Conflict("Email already exists");

        _db.Users.Add(new User
        {
            Email = request.Email,
            PasswordGuid = ConvertPasswordToGuid(request.Password)
        });
        await _db.SaveChangesAsync();

        return Ok("Registered successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var passwordGuid = ConvertPasswordToGuid(request.Password);
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email &&
            u.PasswordGuid == passwordGuid);

        return user != null
            ? Ok("Login successful")
            : Unauthorized("Invalid credentials");
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return NotFound();

        user.ResetToken = Guid.NewGuid();
        user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();

        return Ok(new { token = user.ResetToken });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u =>
            u.Email == request.Email &&
            u.ResetToken == request.Token &&
            u.ResetTokenExpiry > DateTime.UtcNow);

        if (user == null) return BadRequest("Invalid token");

        user.PasswordGuid = ConvertPasswordToGuid(request.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;
        await _db.SaveChangesAsync();

        return Ok("Password reset successful");
    }

    private static Guid ConvertPasswordToGuid(string password)
    {
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            byte[] guidBytes = new byte[16];
            Array.Copy(hashBytes, guidBytes, 16);
            return new Guid(guidBytes);
        }
    }
}