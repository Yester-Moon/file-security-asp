namespace AdminService.Application.DTOs;

public record AdminDashboardStats
{
    public int TotalUsers { get; init; }
    public int ActiveUsers { get; init; }
    public int TotalFiles { get; init; }
    public long TotalStorageUsed { get; init; }
    public int TotalShares { get; init; }
    public int QuarantinedFiles { get; init; }
}

public record UserManagementDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public List<string> Roles { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
    public int FileCount { get; init; }
    public long StorageUsed { get; init; }
}

public record FileAuditDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public string OwnerEmail { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public DateTime CreatedAt { get; init; }
    public int ShareCount { get; init; }
    public int AccessCount { get; init; }
}

public record UpdateUserStatusRequest
{
    public bool IsActive { get; init; }
}

public record ResetPasswordRequest
{
    public string NewPassword { get; init; } = string.Empty;
}
