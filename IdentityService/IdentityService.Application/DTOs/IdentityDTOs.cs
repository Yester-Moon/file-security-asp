namespace IdentityService.Application.DTOs;

public record RegisterRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
}

public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}

public record LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
    public DateTime ExpiresAt { get; init; }
}

public record UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? PhoneNumber { get; init; }
    public bool IsEmailVerified { get; init; }
    public bool IsActive { get; init; }
    public List<string> Roles { get; init; } = new();
    public DateTime CreatedAt { get; init; }
}
