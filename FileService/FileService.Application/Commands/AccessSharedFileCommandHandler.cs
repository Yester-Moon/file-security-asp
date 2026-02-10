using MediatR;
using FileService.Domain.Repositories;
using FileService.Domain.Services;
using FileService.Domain.Enums;
using FileService.Domain.IntegrationEvents.Events;

namespace FileService.Application.Commands;

public class AccessSharedFileCommandHandler : IRequestHandler<AccessSharedFileCommand, DownloadFileResult>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileShareRepository _shareRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly IMediator _mediator;
    private readonly ILogger<AccessSharedFileCommandHandler> _logger;

    public AccessSharedFileCommandHandler(
        IFileRepository fileRepository,
        IFileShareRepository shareRepository,
        IEncryptionService encryptionService,
        IMediator mediator,
        ILogger<AccessSharedFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _shareRepository = shareRepository;
        _encryptionService = encryptionService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<DownloadFileResult> Handle(AccessSharedFileCommand request, CancellationToken cancellationToken)
    {
        var share = await _shareRepository.GetByTokenAsync(request.Token, cancellationToken);
        
        if (share == null)
            throw new InvalidOperationException("Share link not found");

        if (!share.CanAccess())
            throw new InvalidOperationException("Share link is expired or has reached maximum access count");

        if (share.Settings.RequiresPassword())
        {
            if (string.IsNullOrEmpty(request.Password))
                throw new UnauthorizedAccessException("Password is required");

            if (!BCrypt.Net.BCrypt.Verify(request.Password, share.Settings.Password))
                throw new UnauthorizedAccessException("Invalid password");
        }

        var file = await _fileRepository.GetByIdAsync(share.FileId, cancellationToken);
        
        if (file == null)
            throw new FileNotFoundException("File not found");

        if (file.Status != FileStatus.Ready)
            throw new InvalidOperationException($"File is not ready. Current status: {file.Status}");

        if (file.EncryptionInfo == null)
            throw new InvalidOperationException("File encryption information is missing");

        _logger.LogInformation(
            "Accessing shared file {FileId} via token {Token} from IP {IpAddress}",
            file.Id, request.Token, request.IpAddress);

        share.RecordAccess(request.IpAddress, request.UserAgent, request.UserId);
        await _shareRepository.UpdateAsync(share, cancellationToken);

        var decryptedStream = await _encryptionService.DecryptFileAsync(
            file.EncryptionInfo.EncryptedPath,
            file.EncryptionInfo.KeyIdentifier,
            cancellationToken);

        await _mediator.Publish(new FileAccessedIntegrationEvent(
            file.Id,
            "SharedAccess",
            request.UserId,
            request.IpAddress), cancellationToken);

        return new DownloadFileResult
        {
            FileStream = decryptedStream,
            FileName = file.Metadata.FileName,
            ContentType = file.Metadata.ContentType,
            FileSize = file.Metadata.FileSize
        };
    }
}
