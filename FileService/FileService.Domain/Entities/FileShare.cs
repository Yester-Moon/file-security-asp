using FileService.Domain.Common;
using FileService.Domain.ValueObjects;

namespace FileService.Domain.Entities;

public class FileShare : Entity
{
    public Guid FileId { get; private set; }
    public string Token { get; private set; }
    public ShareSettings Settings { get; private set; }
    public int AccessCount { get; private set; }
    public bool IsActive { get; private set; }

    private readonly List<ShareAccess> _accessHistory = new();
    public IReadOnlyCollection<ShareAccess> AccessHistory => _accessHistory.AsReadOnly();

    private FileShare() { }

    public FileShare(Guid fileId, ShareSettings settings, string token)
    {
        FileId = fileId;
        Settings = settings;
        Token = token;
        AccessCount = 0;
        IsActive = true;
    }

    public bool CanAccess()
    {
        if (!IsActive) return false;
        if (Settings.IsExpired()) return false;
        if (Settings.HasReachedMaxAccess(AccessCount)) return false;
        return true;
    }

    public void RecordAccess(string? ipAddress, string? userAgent, Guid? userId = null)
    {
        if (!CanAccess())
            throw new InvalidOperationException("Share link is not accessible");

        var access = new ShareAccess(Id, ipAddress, userAgent, userId);
        _accessHistory.Add(access);
        AccessCount++;
        UpdateTimestamp();
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdateTimestamp();
    }

    public void Activate()
    {
        IsActive = true;
        UpdateTimestamp();
    }

    public bool ValidatePassword(string password)
    {
        if (!Settings.RequiresPassword())
            return true;

        return Settings.Password == password;
    }
}
