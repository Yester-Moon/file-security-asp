using FileService.Domain.IntegrationEvents;

namespace FileService.Domain.IntegrationEvents.Events;

public class FileUploadedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid FileId { get; }
    public Guid OwnerId { get; }
    public string FileName { get; }
    public long FileSize { get; }

    public FileUploadedIntegrationEvent(Guid fileId, Guid ownerId, string fileName, long fileSize)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        FileId = fileId;
        OwnerId = ownerId;
        FileName = fileName;
        FileSize = fileSize;
    }
}
