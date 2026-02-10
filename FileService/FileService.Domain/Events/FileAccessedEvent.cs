using FileService.Domain.Common;

namespace FileService.Domain.Events;

public class FileAccessedEvent : DomainEvent
{
    public Guid FileId { get; }
    public Guid ShareId { get; }
    public Guid? UserId { get; }
    public string? IpAddress { get; }
    public string Action { get; }

    public FileAccessedEvent(Guid fileId, Guid shareId, string action, Guid? userId = null, string? ipAddress = null)
    {
        FileId = fileId;
        ShareId = shareId;
        UserId = userId;
        IpAddress = ipAddress;
        Action = action;
    }
}
