using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FileService.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // 简化的登录验证，生产环境需要连接 IdentityService
        // TODO: 连接 IdentityService 进行真实的用户验证
        
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        // 演示用途：任何登录都会成功（生产环境必须验证）
        var userId = Guid.NewGuid();
        var token = GenerateJwtToken(userId, request.Email, new[] { "User" });

        _logger.LogInformation("User logged in: {Email}", request.Email);

        return Ok(new LoginResponse
        {
            Token = token,
            UserId = userId,
            Email = request.Email,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        });
    }

    [HttpPost("register")]
    public IActionResult Register([FromBody] RegisterRequest request)
    {
        // 简化的注册，生产环境需要连接 IdentityService
        // TODO: 连接 IdentityService 进行用户注册
        
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        _logger.LogInformation("User registered: {Email}", request.Email);

        return Ok(new { Message = "User registered successfully" });
    }

    [HttpPost("admin-login")]
    public IActionResult AdminLogin([FromBody] LoginRequest request)
    {
        // 简化的管理员登录
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
        {
            return BadRequest("Email and password are required");
        }

        // 演示用途：检查是否为管理员邮箱
        if (request.Email.Contains("admin"))
        {
            var userId = Guid.NewGuid();
            var token = GenerateJwtToken(userId, request.Email, new[] { "Admin" });

            _logger.LogInformation("Admin logged in: {Email}", request.Email);

            return Ok(new LoginResponse
            {
                Token = token,
                UserId = userId,
                Email = request.Email,
                ExpiresAt = DateTime.UtcNow.AddHours(1)
            });
        }

        return Unauthorized("Invalid admin credentials");
    }

    private string GenerateJwtToken(Guid userId, string email, string[] roles)
    {
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
