using FileService.Infrastructure.IntegrationEvents;

namespace FileService.Infrastructure.IntegrationEvents.Events;

public class FileSharedIntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; }
    public DateTime OccurredOn { get; }
    public Guid FileId { get; }
    public Guid ShareId { get; }
    public Guid OwnerId { get; }
    public string ShareToken { get; }

    public FileSharedIntegrationEvent(Guid fileId, Guid shareId, Guid ownerId, string shareToken)
    {
        EventId = Guid.NewGuid();
        OccurredOn = DateTime.UtcNow;
        FileId = fileId;
        ShareId = shareId;
        OwnerId = ownerId;
        ShareToken = shareToken;
    }
}
