using FileService.Domain.Common;

namespace FileService.Domain.Entities;

public class Folder : Entity, IAggregateRoot
{
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public Guid? ParentFolderId { get; private set; }
    public string Path { get; private set; }

    private Folder() { }

    public Folder(string name, Guid ownerId, Guid? parentFolderId = null, string? parentPath = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Folder name cannot be empty", nameof(name));

        Name = name;
        OwnerId = ownerId;
        ParentFolderId = parentFolderId;
        Path = string.IsNullOrEmpty(parentPath) ? $"/{name}" : $"{parentPath}/{name}";
    }

    public void Rename(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            throw new ArgumentException("Folder name cannot be empty", nameof(newName));

        Name = newName;
        UpdateTimestamp();
    }

    public void MoveTo(Guid? targetParentId, string? targetParentPath = null)
    {
        ParentFolderId = targetParentId;
        Path = string.IsNullOrEmpty(targetParentPath) ? $"/{Name}" : $"{targetParentPath}/{Name}";
        UpdateTimestamp();
    }
}
