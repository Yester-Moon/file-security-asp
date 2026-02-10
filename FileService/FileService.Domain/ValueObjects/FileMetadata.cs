namespace FileService.Domain.ValueObjects;

public record FileMetadata
{
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public long FileSize { get; init; }
    public string Extension { get; init; }
    public string Hash { get; init; }

    public FileMetadata(string fileName, string contentType, long fileSize, string hash)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));
        
        if (fileSize <= 0)
            throw new ArgumentException("File size must be positive", nameof(fileSize));

        FileName = fileName;
        ContentType = contentType;
        FileSize = fileSize;
        Extension = Path.GetExtension(fileName);
        Hash = hash;
    }
}
