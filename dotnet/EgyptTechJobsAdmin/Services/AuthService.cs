using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EgyptTechJobsAdmin.Data;
using EgyptTechJobsAdmin.Models.Entities;

namespace EgyptTechJobsAdmin.Services;

public interface IAuthService
{
    Task<AuthResult> LoginAsync(string email, string password);
    Task<AdminUser?> GetUserByIdAsync(int id);
    Task SeedAdminUsersAsync();
}

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<AuthResult> LoginAsync(string email, string password)
    {
        var user = await _context.AdminUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsActive);

        if (user == null)
        {
            return new AuthResult { Success = false, Message = "Invalid email or password" };
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new AuthResult { Success = false, Message = "Invalid email or password" };
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        // Generate JWT token
        var token = GenerateJwtToken(user);

        return new AuthResult
        {
            Success = true,
            Message = "Login successful",
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName
            }
        };
    }

    public async Task<AdminUser?> GetUserByIdAsync(int id)
    {
        return await _context.AdminUsers.FindAsync(id);
    }

    public async Task SeedAdminUsersAsync()
    {
        var adminUsers = new[]
        {
            new { Email = "diaadawood@techjobs.com", Username = "diaadawood", FullName = "Diaa Dawood" },
            new { Email = "mohamedabdelmohsen@techjobs.com", Username = "mohamedabdelmohsen", FullName = "Mohamed Abdelmohsen" },
            new { Email = "marwanemad@techjobs.com", Username = "marwanemad", FullName = "Marwan Emad" }
        };

        foreach (var admin in adminUsers)
        {
            var exists = await _context.AdminUsers.AnyAsync(u => u.Email == admin.Email);
            if (!exists)
            {
                var user = new AdminUser
                {
                    Email = admin.Email,
                    Username = admin.Username,
                    FullName = admin.FullName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"), // Default password
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                _context.AdminUsers.Add(user);
            }
        }

        await _context.SaveChangesAsync();
    }

    private string GenerateJwtToken(AdminUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? "TechJobsAdminSecretKey2024SuperSecure!"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim("fullName", user.FullName ?? user.Username)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "TechJobsAdmin",
            audience: _configuration["Jwt:Audience"] ?? "TechJobsAdminPortal",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public UserDto? User { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? FullName { get; set; }
}
