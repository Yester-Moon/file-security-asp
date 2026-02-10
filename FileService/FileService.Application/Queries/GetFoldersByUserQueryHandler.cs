using MediatR;
using FileService.Application.DTOs;
using FileService.Domain.Repositories;

namespace FileService.Application.Queries;

public class GetFoldersByUserQueryHandler : IRequestHandler<GetFoldersByUserQuery, IEnumerable<FolderDto>>
{
    private readonly IFolderRepository _folderRepository;

    public GetFoldersByUserQueryHandler(IFolderRepository folderRepository)
    {
        _folderRepository = folderRepository;
    }

    public async Task<IEnumerable<FolderDto>> Handle(GetFoldersByUserQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<FileService.Domain.Entities.Folder> folders;

        if (request.ParentFolderId.HasValue)
        {
            folders = await _folderRepository.GetByParentIdAsync(request.ParentFolderId.Value, cancellationToken);
        }
        else
        {
            var allFolders = await _folderRepository.GetByOwnerIdAsync(request.UserId, cancellationToken);
            folders = allFolders.Where(f => f.ParentFolderId == null);
        }

        return folders.Select(f => new FolderDto
        {
            Id = f.Id,
            Name = f.Name,
            Path = f.Path,
            ParentFolderId = f.ParentFolderId,
            CreatedAt = f.CreatedAt
        });
    }
}
