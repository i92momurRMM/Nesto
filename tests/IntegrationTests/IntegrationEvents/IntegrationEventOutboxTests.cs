using Nesto.Contracts.IntegrationEvents.V1;
using Nesto.Domain.Users;
using Nesto.Infrastructure.Database;
using Nesto.Infrastructure.DomainEvents;
using Nesto.Infrastructure.IntegrationEvents;
using Nesto.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Nesto.IntegrationTests.IntegrationEvents;

public sealed class IntegrationEventOutboxTests(IntegrationTestWebAppFactory factory)
    : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task ReplayedDomainEvent_IsHandledOnceAndKeepsItsIntegrationEventIdentity()
    {
        DateTime startedOnUtc = DateTime.UtcNow;
        Guid userId = await RegisterUserAsync(UniqueEmail());

        DateTime deadline = DateTime.UtcNow.AddSeconds(20);
        OutboxMessage? domainMessage = null;

        while (domainMessage is null && DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            using IServiceScope scope = Services.CreateScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            List<OutboxMessage> candidates = await context.OutboxMessages
                .AsNoTracking()
                .Where(message =>
                    message.OccurredOnUtc >= startedOnUtc &&
                    message.Type.Contains(nameof(UserRegisteredDomainEvent)) &&
                    message.ProcessedOnUtc != null)
                .ToListAsync();

            domainMessage = candidates.SingleOrDefault(message =>
                ((UserRegisteredDomainEvent)message.Deserialize()).UserId == userId);
        }

        domainMessage.ShouldNotBeNull();
        Guid integrationEventId;

        using (IServiceScope scope = Services.CreateScope())
        {
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            (await context.ProcessedDomainEventHandlers.CountAsync(
                processed => processed.EventId == domainMessage.Id)).ShouldBe(1);

            List<IntegrationOutboxMessage> integrationMessages = await context.IntegrationOutboxMessages
                .AsNoTracking()
                .Where(message => message.OccurredOnUtc >= startedOnUtc)
                .Where(message => message.Type.Contains(nameof(UserRegisteredIntegrationEvent)))
                .ToListAsync();

            integrationEventId = integrationMessages.Single(message =>
                ((UserRegisteredIntegrationEvent)message.Deserialize()).UserId == userId).Id;

            await context.OutboxMessages
                .Where(message => message.Id == domainMessage.Id)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(message => message.ProcessedOnUtc, (DateTime?)null));
        }

        deadline = DateTime.UtcNow.AddSeconds(20);
        bool wasReplayed = false;

        while (!wasReplayed && DateTime.UtcNow < deadline)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            using IServiceScope scope = Services.CreateScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            wasReplayed = await context.OutboxMessages.AnyAsync(message =>
                message.Id == domainMessage.Id &&
                message.ProcessedOnUtc != null &&
                message.Attempts == 2);
        }

        wasReplayed.ShouldBeTrue();

        using (IServiceScope scope = Services.CreateScope())
        {
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            (await context.ProcessedDomainEventHandlers.CountAsync(
                processed => processed.EventId == domainMessage.Id)).ShouldBe(1);
            (await context.IntegrationOutboxMessages.CountAsync(
                message => message.Id == integrationEventId)).ShouldBe(1);
        }
    }
}
