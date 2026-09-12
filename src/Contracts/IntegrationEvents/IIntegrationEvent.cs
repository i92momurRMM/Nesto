namespace Nesto.Contracts.IntegrationEvents;

public interface IIntegrationEvent
{
    Guid Id { get; }

    DateTime OccurredOnUtc { get; }
}
