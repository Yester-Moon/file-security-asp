using MediatR;
using FileService.Application.DTOs;
using FileService.Domain.Entities;
using FileService.Domain.Repositories;

namespace FileService.Application.Commands;

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, FolderDto>
{
    private readonly IFolderRepository _folderRepository;
    private readonly ILogger<CreateFolderCommandHandler> _logger;

    public CreateFolderCommandHandler(
        IFolderRepository folderRepository,
        ILogger<CreateFolderCommandHandler> logger)
    {
        _folderRepository = folderRepository;
        _logger = logger;
    }

    public async Task<FolderDto> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        string? parentPath = null;
        
        if (request.ParentFolderId.HasValue)
        {
            var parentFolder = await _folderRepository.GetByIdAsync(request.ParentFolderId.Value, cancellationToken);
            if (parentFolder == null)
                throw new InvalidOperationException("Parent folder not found");
            
            if (parentFolder.OwnerId != request.OwnerId)
                throw new UnauthorizedAccessException("Cannot create folder in another user's folder");
            
            parentPath = parentFolder.Path;
        }

        var folder = new Folder(request.Name, request.OwnerId, request.ParentFolderId, parentPath);
        await _folderRepository.AddAsync(folder, cancellationToken);

        _logger.LogInformation("Folder {FolderName} created by user {UserId}", request.Name, request.OwnerId);

        return new FolderDto
        {
            Id = folder.Id,
            Name = folder.Name,
            Path = folder.Path,
            ParentFolderId = folder.ParentFolderId,
            CreatedAt = folder.CreatedAt
        };
    }
}
