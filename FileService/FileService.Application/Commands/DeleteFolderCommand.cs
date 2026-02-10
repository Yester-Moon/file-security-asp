using MediatR;

namespace FileService.Application.Commands;

public record DeleteFolderCommand : IRequest<Unit>
{
    public Guid FolderId { get; init; }
    public Guid UserId { get; init; }
}
