using FileService.Domain.Common;

namespace FileService.Domain.Entities;

public class ShareAccess : Entity
{
    public Guid ShareId { get; private set; }
    public Guid? UserId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Location { get; private set; }

    private ShareAccess() { }

    public ShareAccess(Guid shareId, string? ipAddress, string? userAgent, Guid? userId = null)
    {
        ShareId = shareId;
        UserId = userId;
        IpAddress = ipAddress;
        UserAgent = userAgent;
    }

    public void SetLocation(string location)
    {
        Location = location;
        UpdateTimestamp();
    }
}
