namespace AuditService.Application.DTOs;

public record CreateAuditLogRequest
{
    public Guid? UserId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public Guid? EntityId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Details { get; init; }
    public bool IsSuccess { get; init; } = true;
    public string? ErrorMessage { get; init; }
}

public record AuditLogDto
{
    public Guid Id { get; init; }
    public Guid? UserId { get; init; }
    public string ServiceName { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public Guid? EntityId { get; init; }
    public string? IpAddress { get; init; }
    public DateTime Timestamp { get; init; }
    public bool IsSuccess { get; init; }
}

public record AuditQueryRequest
{
    public Guid? UserId { get; init; }
    public string? ServiceName { get; init; }
    public string? Action { get; init; }
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
