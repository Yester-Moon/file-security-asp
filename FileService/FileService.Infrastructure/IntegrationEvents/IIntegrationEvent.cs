using MediatR;

namespace FileService.Infrastructure.IntegrationEvents;

public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
