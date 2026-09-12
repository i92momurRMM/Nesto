using System.Text.Json;
using Nesto.Contracts.IntegrationEvents;

namespace Nesto.Infrastructure.IntegrationEvents;

public sealed class IntegrationOutboxMessage
{
    private IntegrationOutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public int Attempts { get; private set; }

    public static IntegrationOutboxMessage Create(IIntegrationEvent integrationEvent) =>
        new()
        {
            Id = integrationEvent.Id,
            OccurredOnUtc = integrationEvent.OccurredOnUtc,
            Type = integrationEvent.GetType().AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType())
        };

    public IIntegrationEvent Deserialize()
    {
        Type integrationEventType = System.Type.GetType(Type, throwOnError: true)!;
        return (IIntegrationEvent)JsonSerializer.Deserialize(Content, integrationEventType)!;
    }

    public void MarkProcessed(DateTime processedOnUtc)
    {
        ProcessedOnUtc = processedOnUtc;
        Error = null;
        Attempts++;
    }

    public void MarkFailed(string error)
    {
        Error = error;
        Attempts++;
    }
}
