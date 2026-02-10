using MediatR;

namespace FileService.Application.Commands;

public record DeleteFileCommand : IRequest<Unit>
{
    public Guid FileId { get; init; }
    public Guid UserId { get; init; }
}
