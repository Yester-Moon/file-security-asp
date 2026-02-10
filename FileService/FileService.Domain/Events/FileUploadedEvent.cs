using FileService.Domain.Common;

namespace FileService.Domain.Events;

public class FileUploadedEvent : DomainEvent
{
    public Guid FileId { get; }
    public Guid OwnerId { get; }
    public string FileName { get; }
    public long FileSize { get; }

    public FileUploadedEvent(Guid fileId, Guid ownerId, string fileName, long fileSize)
    {
        FileId = fileId;
        OwnerId = ownerId;
        FileName = fileName;
        FileSize = fileSize;
    }
}
