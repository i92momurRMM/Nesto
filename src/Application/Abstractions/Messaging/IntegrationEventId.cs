using System.Security.Cryptography;
using System.Text;
using Nesto.SharedKernel;

namespace Nesto.Application.Abstractions.Messaging;

internal static class IntegrationEventId
{
    public static Guid From<TIntegrationEvent>(DomainEventContext context)
    {
        string input = $"{context.EventId:N}:{typeof(TIntegrationEvent).FullName}";
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(input));

        return new Guid(hash.AsSpan(0, 16));
    }
}
