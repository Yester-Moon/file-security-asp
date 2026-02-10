using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Commands;

public record AccessSharedFileCommand : IRequest<DownloadFileResult>
{
    public string Token { get; init; } = string.Empty;
    public string? Password { get; init; }
    public Guid? UserId { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
}
