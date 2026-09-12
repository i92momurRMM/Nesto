using Nesto.Infrastructure.Database;
using Nesto.Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nesto.Infrastructure.IntegrationEvents;

internal sealed class IntegrationOutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<IntegrationOutboxProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(options.Value.IntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await ProcessBatchAsync(stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = scopeFactory.CreateScope();
        ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        IIntegrationEventTransport transport =
            scope.ServiceProvider.GetRequiredService<IIntegrationEventTransport>();

        await using IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<IntegrationOutboxMessage> messages = await context.IntegrationOutboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM public.integration_outbox_messages
                WHERE processed_on_utc IS NULL AND attempts < {options.Value.MaxAttempts}
                ORDER BY occurred_on_utc
                LIMIT {options.Value.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .ToListAsync(cancellationToken);

        foreach (IntegrationOutboxMessage message in messages)
        {
            try
            {
                await transport.PublishAsync(message.Deserialize(), cancellationToken);
                message.MarkProcessed(DateTime.UtcNow);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to publish integration event {MessageId}", message.Id);
                message.MarkFailed(exception.Message);
            }
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
