using MediatR;

namespace FileService.Application.Commands;

public record DownloadFileCommand : IRequest<DownloadFileResult>
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
}

public record DownloadFileResult
{
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
}
