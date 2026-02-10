namespace FileService.Domain.Services;

public interface IVirusScanService
{
    Task<string> ScanFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
}
