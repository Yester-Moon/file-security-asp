namespace IdentityService.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public List<string> Roles { get; private set; } = new();

    private User() { }

    public User(string email, string passwordHash, string fullName, string? phoneNumber = null)
    {
        Id = Guid.NewGuid();
        Email = email.ToLowerInvariant();
        PasswordHash = passwordHash;
        FullName = fullName;
        PhoneNumber = phoneNumber;
        IsEmailVerified = false;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        Roles = new List<string> { "User" };
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void AddRole(string role)
    {
        if (!Roles.Contains(role))
        {
            Roles.Add(role);
        }
    }

    public void RemoveRole(string role)
    {
        Roles.Remove(role);
    }

    public bool HasRole(string role)
    {
        return Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }
}
