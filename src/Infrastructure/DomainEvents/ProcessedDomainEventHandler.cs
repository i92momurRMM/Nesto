namespace Nesto.Infrastructure.DomainEvents;

public sealed class ProcessedDomainEventHandler
{
    private ProcessedDomainEventHandler()
    {
    }

    public Guid EventId { get; private set; }

    public string HandlerName { get; private set; } = string.Empty;

    public DateTime ProcessedOnUtc { get; private set; }

    public static ProcessedDomainEventHandler Create(
        Guid eventId,
        string handlerName,
        DateTime processedOnUtc) =>
        new()
        {
            EventId = eventId,
            HandlerName = handlerName,
            ProcessedOnUtc = processedOnUtc
        };
}
