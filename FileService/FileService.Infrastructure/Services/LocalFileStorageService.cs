using FileService.Domain.Services;

namespace FileService.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storagePath;
    private readonly ILogger<LocalFileStorageService> _logger;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _storagePath = configuration["FileStorage:Path"] ?? Path.Combine(Directory.GetCurrentDirectory(), "FileStorage");
        _logger = logger;

        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fileId = Guid.NewGuid().ToString();
        var extension = Path.GetExtension(fileName);
        var safeFileName = $"{fileId}{extension}";
        var filePath = Path.Combine(_storagePath, safeFileName);

        _logger.LogInformation("Saving file: {FileName} to {FilePath}", fileName, filePath);

        using var fileStreamOutput = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fileStreamOutput, cancellationToken);

        return filePath;
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
        {
            _logger.LogWarning("File not found: {FilePath}", filePath);
            throw new FileNotFoundException("File not found", filePath);
        }

        var memoryStream = new MemoryStream();
        using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string filePath, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(File.Exists(filePath));
    }
}
