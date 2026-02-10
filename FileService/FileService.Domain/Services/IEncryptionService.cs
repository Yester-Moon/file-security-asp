using FileService.Domain.ValueObjects;

namespace FileService.Domain.Services;

public interface IEncryptionService
{
    Task<EncryptionInfo> EncryptFileAsync(Stream fileStream, string outputPath, CancellationToken cancellationToken = default);
    Task<Stream> DecryptFileAsync(string encryptedPath, string keyIdentifier, CancellationToken cancellationToken = default);
}
