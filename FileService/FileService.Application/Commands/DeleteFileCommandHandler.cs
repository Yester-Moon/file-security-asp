using MediatR;
using FileService.Domain.Repositories;
using FileService.Domain.Services;
using FileService.Domain.Enums;
using FileService.Domain.IntegrationEvents.Events;

namespace FileService.Application.Commands;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Unit>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IMediator _mediator;
    private readonly ILogger<DeleteFileCommandHandler> _logger;

    public DeleteFileCommandHandler(
        IFileRepository fileRepository,
        IFileStorageService storageService,
        IMediator mediator,
        ILogger<DeleteFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _storageService = storageService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
        
        if (file == null)
            throw new FileNotFoundException("File not found");

        if (!file.HasPermission(request.UserId, PermissionType.Delete) && file.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("User does not have permission to delete this file");

        _logger.LogInformation("Deleting file {FileId} by user {UserId}", request.FileId, request.UserId);

        if (file.EncryptionInfo != null)
        {
            await _storageService.DeleteFileAsync(file.EncryptionInfo.EncryptedPath, cancellationToken);
        }

        await _fileRepository.DeleteAsync(request.FileId, cancellationToken);

        await _mediator.Publish(new FileAccessedIntegrationEvent(
            file.Id,
            "Delete",
            request.UserId), cancellationToken);

        return Unit.Value;
    }
}
