namespace AuditService.Domain.Entities;

public class AuditLog
{
    public Guid Id { get; private set; }
    public Guid? UserId { get; private set; }
    public string ServiceName { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string EntityType { get; private set; } = string.Empty;
    public Guid? EntityId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Details { get; private set; }
    public DateTime Timestamp { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }

    private AuditLog() { }

    public AuditLog(
        string serviceName,
        string action,
        string entityType,
        Guid? entityId = null,
        Guid? userId = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? details = null,
        bool isSuccess = true,
        string? errorMessage = null)
    {
        Id = Guid.NewGuid();
        ServiceName = serviceName;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        UserId = userId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Details = details;
        Timestamp = DateTime.UtcNow;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }
}
