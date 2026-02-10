using FileService.Domain.IntegrationEvents;

namespace FileService.Domain.IntegrationEvents.Events;

public class FileAccessedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid FileId { get; }
    public Guid? UserId { get; }
    public string? IpAddress { get; }
    public string Action { get; }

    public FileAccessedIntegrationEvent(Guid fileId, string action, Guid? userId = null, string? ipAddress = null)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        FileId = fileId;
        UserId = userId;
        IpAddress = ipAddress;
        Action = action;
    }
}
