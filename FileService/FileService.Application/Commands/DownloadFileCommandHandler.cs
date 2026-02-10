using MediatR;
using FileService.Domain.Repositories;
using FileService.Domain.Services;
using FileService.Domain.Enums;
using FileService.Domain.IntegrationEvents.Events;

namespace FileService.Application.Commands;

public class DownloadFileCommandHandler : IRequestHandler<DownloadFileCommand, DownloadFileResult>
{
    private readonly IFileRepository _fileRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMediator _mediator;
    private readonly ILogger<DownloadFileCommandHandler> _logger;

    public DownloadFileCommandHandler(
        IFileRepository fileRepository,
        IEncryptionService encryptionService,
        IMediator mediator,
        ILogger<DownloadFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _encryptionService = encryptionService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<DownloadFileResult> Handle(DownloadFileCommand request, CancellationToken cancellationToken)
    {
        var file = await _fileRepository.GetByIdAsync(request.FileId, cancellationToken);
        
        if (file == null)
            throw new FileNotFoundException("File not found");

        if (!file.HasPermission(request.UserId, PermissionType.Download) && file.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("User does not have permission to download this file");

        if (file.Status != FileStatus.Ready)
            throw new InvalidOperationException($"File is not ready for download. Current status: {file.Status}");

        if (file.EncryptionInfo == null)
            throw new InvalidOperationException("File encryption information is missing");

        _logger.LogInformation("Downloading file {FileId} for user {UserId}", request.FileId, request.UserId);

        var decryptedStream = await _encryptionService.DecryptFileAsync(
            file.EncryptionInfo.EncryptedPath,
            file.EncryptionInfo.KeyIdentifier,
            cancellationToken);

        await _mediator.Publish(new FileAccessedIntegrationEvent(
            file.Id,
            "Download",
            request.UserId), cancellationToken);

        return new DownloadFileResult
        {
            FileStream = decryptedStream,
            FileName = file.Metadata.FileName,
            ContentType = file.Metadata.ContentType,
            FileSize = file.Metadata.FileSize
        };
    }
}
