using FileService.Domain.IntegrationEvents.Events;
// using AuditService.Application.DTOs;  // TODO: Á¬½Óµ½ AuditService
using MediatR;

namespace FileService.Application.IntegrationEventHandlers;

public class FileAccessedIntegrationEventHandler : INotificationHandler<FileAccessedIntegrationEvent>
{
    private readonly ILogger<FileAccessedIntegrationEventHandler> _logger;

    public FileAccessedIntegrationEventHandler(ILogger<FileAccessedIntegrationEventHandler> logger)
    {
        _logger = logger;
    }

    public async Task Handle(FileAccessedIntegrationEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "File accessed event received: FileId={FileId}, UserId={UserId}, Action={Action}",
            notification.FileId,
            notification.UserId,
            notification.Action);

        // TODO: Send to AuditService
        /*
        var auditLog = new CreateAuditLogRequest
        {
            UserId = notification.UserId,
            ServiceName = "FileService",
            Action = notification.Action,
            EntityType = "File",
            EntityId = notification.FileId,
            IpAddress = notification.IpAddress,
            IsSuccess = true
        };
        */

        await Task.CompletedTask;
    }
}
