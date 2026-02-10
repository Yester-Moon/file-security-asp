namespace FileService.Application.DTOs;

public record UploadFileRequest
{
    public Guid UserId { get; init; }
    public Guid? FolderId { get; init; }
}

public record UploadFileResponse
{
    public Guid FileId { get; init; }
    public string FileName { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime UploadedAt { get; init; }
}

public record FileDto
{
    public Guid Id { get; init; }
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public string Extension { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public Guid? FolderId { get; init; }
}

public record CreateShareRequest
{
    public DateTime? ExpirationDate { get; init; }
    public int? MaxAccessCount { get; init; }
    public string? Password { get; init; }
    public bool RequireAuthentication { get; init; }
}

public record ShareLinkResponse
{
    public Guid ShareId { get; init; }
    public string Token { get; init; } = string.Empty;
    public string ShareUrl { get; init; } = string.Empty;
    public DateTime? ExpirationDate { get; init; }
    public int? MaxAccessCount { get; init; }
    public bool RequiresPassword { get; init; }
}

public record ShareAccessDto
{
    public Guid Id { get; init; }
    public DateTime AccessedAt { get; init; }
    public string? IpAddress { get; init; }
    public string? Location { get; init; }
    public Guid? UserId { get; init; }
}

public record CreateFolderRequest
{
    public string Name { get; init; } = string.Empty;
    public Guid? ParentFolderId { get; init; }
}

public record FolderDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public Guid? ParentFolderId { get; init; }
    public DateTime CreatedAt { get; init; }
}
