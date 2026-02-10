using System.Security.Cryptography;
using FileService.Domain.Services;
using FileService.Domain.ValueObjects;

namespace FileService.Infrastructure.Services;

public class AesEncryptionService : IEncryptionService
{
    private const string Algorithm = "AES-256-CBC";
    private readonly byte[] _key;
    private readonly byte[] _iv;

    public AesEncryptionService(IConfiguration configuration)
    {
        // In production, these should be loaded from secure key management service (Azure Key Vault, AWS KMS, etc.)
        var keyBase64 = configuration["Encryption:Key"] ?? throw new InvalidOperationException("Encryption key not configured");
        var ivBase64 = configuration["Encryption:IV"] ?? throw new InvalidOperationException("Encryption IV not configured");
        
        _key = Convert.FromBase64String(keyBase64);
        _iv = Convert.FromBase64String(ivBase64);
    }

    public async Task<EncryptionInfo> EncryptFileAsync(Stream fileStream, string outputPath, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var encryptor = aes.CreateEncryptor();
        using var outputFileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
        using var cryptoStream = new CryptoStream(outputFileStream, encryptor, CryptoStreamMode.Write);

        await fileStream.CopyToAsync(cryptoStream, cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);

        var keyIdentifier = Convert.ToBase64String(_key.Take(16).ToArray());
        return new EncryptionInfo(Algorithm, outputPath, keyIdentifier);
    }

    public async Task<Stream> DecryptFileAsync(string encryptedPath, string keyIdentifier, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(encryptedPath))
            throw new FileNotFoundException("Encrypted file not found", encryptedPath);

        var memoryStream = new MemoryStream();
        
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = _iv;

        using var decryptor = aes.CreateDecryptor();
        using var inputFileStream = new FileStream(encryptedPath, FileMode.Open, FileAccess.Read);
        using var cryptoStream = new CryptoStream(inputFileStream, decryptor, CryptoStreamMode.Read);

        await cryptoStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }
}
