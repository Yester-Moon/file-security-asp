using FileService.Domain.IntegrationEvents.Events;
// using AuditService.Application.DTOs;  // TODO: Á¬½Óµ½ AuditService
using MediatR;

namespace FileService.Application.IntegrationEventHandlers;

public class FileSharedIntegrationEventHandler : INotificationHandler<FileSharedIntegrationEvent>
{
    private readonly ILogger<FileSharedIntegrationEventHandler> _logger;

    public FileSharedIntegrationEventHandler(ILogger<FileSharedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FileSharedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "File shared event received: FileId={FileId}, ShareId={ShareId}, Token={Token}",
            notification.FileId,
            notification.ShareId,
            notification.ShareToken);

        // TODO: Send to AuditService
        /*
        var auditLog = new CreateAuditLogRequest
        {
            UserId = notification.OwnerId,
            ServiceName = "FileService",
            Action = "FileShared",
            EntityType = "FileShare",
            EntityId = notification.ShareId,
            Details = $"File shared with token: {notification.ShareToken}",
            IsSuccess = true
        };
        */

        await Task.CompletedTask;
    }
}
