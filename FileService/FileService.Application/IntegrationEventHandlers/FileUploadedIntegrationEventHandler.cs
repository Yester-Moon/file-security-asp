using FileService.Domain.IntegrationEvents;
using FileService.Domain.IntegrationEvents.Events;
// using AuditService.Application.DTOs;  // TODO: Á¬½Óµ½ AuditService
using MediatR;

namespace FileService.Application.IntegrationEventHandlers;

public class FileUploadedIntegrationEventHandler : INotificationHandler<FileUploadedIntegrationEvent>
{
    private readonly ILogger<FileUploadedIntegrationEventHandler> _logger;
    // In production, inject an HTTP client or message bus to communicate with AuditService

    public FileUploadedIntegrationEventHandler(ILogger<FileUploadedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FileUploadedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "File uploaded event received: FileId={FileId}, OwnerId={OwnerId}, FileName={FileName}",
            notification.FileId,
            notification.OwnerId,
            notification.FileName);

        // TODO: Send audit log to AuditService via HTTP or message queue
        /* 
        var auditLog = new CreateAuditLogRequest
        {
            UserId = notification.OwnerId,
            ServiceName = "FileService",
            Action = "FileUploaded",
            EntityType = "File",
            EntityId = notification.FileId,
            Details = $"File '{notification.FileName}' uploaded, size: {notification.FileSize} bytes",
            IsSuccess = true
        };

        await _auditServiceClient.CreateAuditLogAsync(auditLog, cancellationToken);
        */
        
        await Task.CompletedTask;
    }
}
