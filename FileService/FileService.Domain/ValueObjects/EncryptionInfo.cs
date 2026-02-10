namespace FileService.Domain.ValueObjects;

public record EncryptionInfo
{
    public string Algorithm { get; init; }
    public string EncryptedPath { get; init; }
    public string KeyIdentifier { get; init; }
    public DateTime EncryptedAt { get; init; }

    public EncryptionInfo(string algorithm, string encryptedPath, string keyIdentifier)
    {
        if (string.IsNullOrWhiteSpace(algorithm))
            throw new ArgumentException("Algorithm cannot be empty", nameof(algorithm));

        if (string.IsNullOrWhiteSpace(encryptedPath))
            throw new ArgumentException("Encrypted path cannot be empty", nameof(encryptedPath));

        Algorithm = algorithm;
        EncryptedPath = encryptedPath;
        KeyIdentifier = keyIdentifier;
        EncryptedAt = DateTime.UtcNow;
    }
}
