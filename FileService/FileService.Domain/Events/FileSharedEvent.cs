using FileService.Domain.Common;

namespace FileService.Domain.Events;

public class FileSharedEvent : DomainEvent
{
    public Guid FileId { get; }
    public Guid ShareId { get; }
    public Guid OwnerId { get; }
    public string Token { get; }

    public FileSharedEvent(Guid fileId, Guid shareId, Guid ownerId, string token)
    {
        FileId = fileId;
        ShareId = shareId;
        OwnerId = ownerId;
        Token = token;
    }
}
