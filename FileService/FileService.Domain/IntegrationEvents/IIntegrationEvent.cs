using MediatR;

namespace FileService.Domain.IntegrationEvents;

public interface IIntegrationEvent : INotification
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}
