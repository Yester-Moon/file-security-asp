using MediatR;
using FileService.Application.DTOs;

namespace FileService.Application.Queries;

public record GetFoldersByUserQuery : IRequest<IEnumerable<FolderDto>>
{
    public Guid UserId { get; init; }
    public Guid? ParentFolderId { get; init; }
}
