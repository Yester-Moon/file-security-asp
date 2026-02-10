namespace FileService.Domain.ValueObjects;

public record ShareSettings
{
    public DateTime? ExpirationDate { get; init; }
    public int? MaxAccessCount { get; init; }
    public string? Password { get; init; }
    public bool RequireAuthentication { get; init; }

    public ShareSettings(
        DateTime? expirationDate = null,
        int? maxAccessCount = null,
        string? password = null,
        bool requireAuthentication = false)
    {
        if (maxAccessCount.HasValue && maxAccessCount.Value <= 0)
            throw new ArgumentException("Max access count must be positive", nameof(maxAccessCount));

        ExpirationDate = expirationDate;
        MaxAccessCount = maxAccessCount;
        Password = password;
        RequireAuthentication = requireAuthentication;
    }

    public bool IsExpired() => ExpirationDate.HasValue && ExpirationDate.Value < DateTime.UtcNow;

    public bool HasReachedMaxAccess(int currentAccessCount)
    {
        return MaxAccessCount.HasValue && currentAccessCount >= MaxAccessCount.Value;
    }

    public bool RequiresPassword() => !string.IsNullOrEmpty(Password);
}
