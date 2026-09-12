using System.Text.Json;
using Nesto.SharedKernel;

namespace Nesto.Infrastructure.Outbox;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    public Guid Id { get; private set; }

    public DateTime OccurredOnUtc { get; private set; }

    public string Type { get; private set; } = string.Empty;

    public string Content { get; private set; } = string.Empty;

    public DateTime? ProcessedOnUtc { get; private set; }

    public string? Error { get; private set; }

    public int Attempts { get; private set; }

    public static OutboxMessage Create(IDomainEvent domainEvent, DateTime occurredOnUtc) =>
        new()
        {
            Id = Guid.CreateVersion7(),
            OccurredOnUtc = occurredOnUtc,
            Type = domainEvent.GetType().AssemblyQualifiedName!,
            Content = JsonSerializer.Serialize(domainEvent, domainEvent.GetType())
        };

    public IDomainEvent Deserialize()
    {
        Type eventType = System.Type.GetType(Type, throwOnError: true)!;
        return (IDomainEvent)JsonSerializer.Deserialize(Content, eventType)!;
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
