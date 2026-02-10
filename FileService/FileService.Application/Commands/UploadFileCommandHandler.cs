using MediatR;
using FileService.Application.DTOs;
using FileService.Domain.Entities;
using FileService.Domain.Repositories;
using FileService.Domain.Services;
using FileService.Domain.ValueObjects;
using FileService.Domain.IntegrationEvents.Events;
using System.Security.Cryptography;

namespace FileService.Application.Commands;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, UploadFileResponse>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFileStorageService _storageService;
    private readonly IVirusScanService _virusScanService;
    private readonly IEncryptionService _encryptionService;
    private readonly IMediator _mediator;
    private readonly ILogger<UploadFileCommandHandler> _logger;

    public UploadFileCommandHandler(
        IFileRepository fileRepository,
        IFileStorageService storageService,
        IVirusScanService virusScanService,
        IEncryptionService encryptionService,
        IMediator mediator,
        ILogger<UploadFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _storageService = storageService;
        _virusScanService = virusScanService;
        _encryptionService = encryptionService;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task<UploadFileResponse> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting file upload for user {UserId}: {FileName}", request.UserId, request.FileName);

        // Calculate file hash
        string fileHash;
        using (var sha256 = SHA256.Create())
        {
            var hashBytes = await sha256.ComputeHashAsync(request.FileStream, cancellationToken);
            fileHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            request.FileStream.Position = 0;
        }

        // Create file metadata
        var metadata = new FileMetadata(request.FileName, request.ContentType, request.FileSize, fileHash);
        
        // Create file entity
        var fileEntity = new FileEntity(request.UserId, metadata, request.FolderId);
        await _fileRepository.AddAsync(fileEntity, cancellationToken);

        // Start virus scanning
        fileEntity.StartScanning();
        await _fileRepository.UpdateAsync(fileEntity, cancellationToken);

        _ = Task.Run(async () =>
        {
            try
            {
                // Scan for viruses
                var scanResult = await _virusScanService.ScanFileAsync(request.FileStream, request.FileName, cancellationToken);
                fileEntity.CompleteScan(scanResult);
                await _fileRepository.UpdateAsync(fileEntity, cancellationToken);

                if (!scanResult.Contains("CLEAN", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("File {FileId} failed virus scan: {Result}", fileEntity.Id, scanResult);
                    return;
                }

                // Encrypt and store file
                var tempPath = await _storageService.SaveFileAsync(request.FileStream, request.FileName, cancellationToken);
                using var tempStream = await _storageService.GetFileAsync(tempPath, cancellationToken);
                
                var encryptedPath = Path.Combine(Path.GetDirectoryName(tempPath)!, $"encrypted_{Path.GetFileName(tempPath)}");
                var encryptionInfo = await _encryptionService.EncryptFileAsync(tempStream, encryptedPath, cancellationToken);
                
                // Delete temp file
                await _storageService.DeleteFileAsync(tempPath, cancellationToken);

                fileEntity.CompleteEncryption(encryptionInfo);
                await _fileRepository.UpdateAsync(fileEntity, cancellationToken);

                // Publish integration event
                await _mediator.Publish(new FileUploadedIntegrationEvent(
                    fileEntity.Id, 
                    fileEntity.OwnerId, 
                    request.FileName, 
                    request.FileSize), cancellationToken);

                _logger.LogInformation("File {FileId} uploaded and encrypted successfully", fileEntity.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing file upload {FileId}", fileEntity.Id);
                fileEntity.MarkAsFailed();
                await _fileRepository.UpdateAsync(fileEntity, cancellationToken);
            }
        }, cancellationToken);

        return new UploadFileResponse
        {
            FileId = fileEntity.Id,
            FileName = request.FileName,
            FileSize = request.FileSize,
            Status = fileEntity.Status.ToString(),
            UploadedAt = fileEntity.CreatedAt
        };
    }
}
