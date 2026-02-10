using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Commands;

public record CreateFolderCommand : IRequest<FolderDto>
{
    public string Name { get; init; } = string.Empty;
    public Guid OwnerId { get; init; }
    public Guid? ParentFolderId { get; init; }
}
