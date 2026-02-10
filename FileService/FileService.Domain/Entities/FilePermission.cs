using FileService.Domain.Common;
using FileService.Domain.Enums;

namespace FileService.Domain.Entities;

public class FilePermission : Entity
{
    public Guid FileId { get; private set; }
    public Guid UserId { get; private set; }
    public PermissionType Permissions { get; private set; }

    private FilePermission() { }

    public FilePermission(Guid fileId, Guid userId, PermissionType permissions)
    {
        FileId = fileId;
        UserId = userId;
        Permissions = permissions;
    }

    public void UpdatePermissions(PermissionType permissions)
    {
        Permissions = permissions;
        UpdateTimestamp();
    }

    public bool HasPermission(PermissionType requiredPermission)
    {
        return (Permissions & requiredPermission) == requiredPermission;
    }
}
