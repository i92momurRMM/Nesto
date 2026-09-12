using Nesto.Infrastructure.Database;
using Nesto.Infrastructure.DomainEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nesto.SharedKernel;

namespace Nesto.Infrastructure.Outbox;

internal sealed class OutboxProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<OutboxOptions> options,
    ILogger<OutboxProcessor> logger) : BackgroundService
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
        IDomainEventsDispatcher dispatcher = scope.ServiceProvider.GetRequiredService<IDomainEventsDispatcher>();

        await using IDbContextTransaction transaction =
            await context.Database.BeginTransactionAsync(cancellationToken);

        List<Guid> messageIds = await context.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT *
                FROM public.outbox_messages
                WHERE processed_on_utc IS NULL AND attempts < {options.Value.MaxAttempts}
                ORDER BY occurred_on_utc
                LIMIT {options.Value.BatchSize}
                FOR UPDATE SKIP LOCKED
                """)
            .Select(message => message.Id)
            .ToListAsync(cancellationToken);

        context.ChangeTracker.Clear();

        foreach (Guid messageId in messageIds)
        {
            OutboxMessage message = await context.OutboxMessages
                .SingleAsync(candidate => candidate.Id == messageId, cancellationToken);

            try
            {
                DomainEventContext eventContext = new(message.Id, message.OccurredOnUtc);
                await dispatcher.DispatchAsync(message.Deserialize(), eventContext, cancellationToken);
                message.MarkProcessed(DateTime.UtcNow);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Failed to process outbox message {MessageId}", message.Id);

                message = await context.OutboxMessages
                    .SingleAsync(candidate => candidate.Id == messageId, cancellationToken);
                message.MarkFailed(exception.Message);
            }

            await context.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
