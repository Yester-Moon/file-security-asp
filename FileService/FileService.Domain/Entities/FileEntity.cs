using FileService.Domain.Common;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;

namespace FileService.Domain.Entities;

public class FileEntity : Entity, IAggregateRoot
{
    public Guid OwnerId { get; private set; }
    public Guid? FolderId { get; private set; }
    public FileMetadata Metadata { get; private set; }
    public EncryptionInfo? EncryptionInfo { get; private set; }
    public FileStatus Status { get; private set; }
    public string? VirusScanResult { get; private set; }
    public DateTime? ScannedAt { get; private set; }
    
    private readonly List<FileShare> _shares = new();
    public IReadOnlyCollection<FileShare> Shares => _shares.AsReadOnly();

    private readonly List<FilePermission> _permissions = new();
    public IReadOnlyCollection<FilePermission> Permissions => _permissions.AsReadOnly();

    private FileEntity() { }

    public FileEntity(Guid ownerId, FileMetadata metadata, Guid? folderId = null)
    {
        OwnerId = ownerId;
        Metadata = metadata;
        FolderId = folderId;
        Status = FileStatus.Uploading;
    }

    public void StartScanning()
    {
        if (Status != FileStatus.Uploading)
            throw new InvalidOperationException("File must be in Uploading status to start scanning");
        
        Status = FileStatus.Scanning;
        UpdateTimestamp();
    }

    public void CompleteScan(string result)
    {
        if (Status != FileStatus.Scanning)
            throw new InvalidOperationException("File must be in Scanning status");

        VirusScanResult = result;
        ScannedAt = DateTime.UtcNow;
        
        if (result.Contains("threat", StringComparison.OrdinalIgnoreCase))
        {
            Status = FileStatus.Quarantined;
        }
        else
        {
            Status = FileStatus.Encrypting;
        }
        UpdateTimestamp();
    }

    public void CompleteEncryption(EncryptionInfo encryptionInfo)
    {
        if (Status != FileStatus.Encrypting)
            throw new InvalidOperationException("File must be in Encrypting status");

        EncryptionInfo = encryptionInfo;
        Status = FileStatus.Ready;
        UpdateTimestamp();
    }

    public void MarkAsFailed()
    {
        Status = FileStatus.Failed;
        UpdateTimestamp();
    }

    public FileShare CreateShare(ShareSettings settings, string token)
    {
        if (Status != FileStatus.Ready)
            throw new InvalidOperationException("Only ready files can be shared");

        var share = new FileShare(Id, settings, token);
        _shares.Add(share);
        UpdateTimestamp();
        return share;
    }

    public void GrantPermission(Guid userId, PermissionType permissions)
    {
        var existingPermission = _permissions.FirstOrDefault(p => p.UserId == userId);
        if (existingPermission != null)
        {
            existingPermission.UpdatePermissions(permissions);
        }
        else
        {
            _permissions.Add(new FilePermission(Id, userId, permissions));
        }
        UpdateTimestamp();
    }

    public void RevokePermission(Guid userId)
    {
        var permission = _permissions.FirstOrDefault(p => p.UserId == userId);
        if (permission != null)
        {
            _permissions.Remove(permission);
            UpdateTimestamp();
        }
    }

    public bool HasPermission(Guid userId, PermissionType requiredPermission)
    {
        if (userId == OwnerId)
            return true;

        var permission = _permissions.FirstOrDefault(p => p.UserId == userId);
        return permission?.HasPermission(requiredPermission) ?? false;
    }

    public void MoveTo(Guid? targetFolderId)
    {
        FolderId = targetFolderId;
        UpdateTimestamp();
    }
}
