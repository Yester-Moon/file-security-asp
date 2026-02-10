using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Commands;

public record UploadFileCommand : IRequest<UploadFileResponse>
{
    public Guid UserId { get; init; }
    public Stream FileStream { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
    public long FileSize { get; init; }
    public Guid? FolderId { get; init; }
}
