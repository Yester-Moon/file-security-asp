using MediatR;
using FileService.Domain.Repositories;

namespace FileService.Application.Commands;

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Unit>
{
    private readonly IFolderRepository _folderRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<DeleteFolderCommandHandler> _logger;

    public DeleteFolderCommandHandler(
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        ILogger<DeleteFolderCommandHandler> logger)
    {
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
    {
        var folder = await _folderRepository.GetByIdAsync(request.FolderId, cancellationToken);
        
        if (folder == null)
            throw new InvalidOperationException("Folder not found");

        if (folder.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("User does not have permission to delete this folder");

        var filesInFolder = await _fileRepository.GetByFolderIdAsync(request.FolderId, cancellationToken);
        if (filesInFolder.Any())
            throw new InvalidOperationException("Cannot delete folder that contains files. Delete files first.");

        var subFolders = await _folderRepository.GetByParentIdAsync(request.FolderId, cancellationToken);
        if (subFolders.Any())
            throw new InvalidOperationException("Cannot delete folder that contains subfolders. Delete subfolders first.");

        _logger.LogInformation("Deleting folder {FolderId} by user {UserId}", request.FolderId, request.UserId);

        await _folderRepository.DeleteAsync(request.FolderId, cancellationToken);

        return Unit.Value;
    }
}
