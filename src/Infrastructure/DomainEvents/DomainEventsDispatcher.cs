using System.Collections.Concurrent;
using Nesto.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Nesto.SharedKernel;

namespace Nesto.Infrastructure.DomainEvents;

internal sealed class DomainEventsDispatcher(
    IServiceProvider serviceProvider,
    ApplicationDbContext dbContext,
    IDateTimeProvider clock) : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    public async Task DispatchAsync(
        IDomainEvent domainEvent,
        DomainEventContext context,
        CancellationToken cancellationToken = default)
    {
        Type domainEventType = domainEvent.GetType();
        Type handlerType = HandlerTypeDictionary.GetOrAdd(
            domainEventType,
            eventType => typeof(IDomainEventHandler<>).MakeGenericType(eventType));

        IEnumerable<object?> handlers = serviceProvider.GetServices(handlerType);
        IDbContextTransaction transaction = dbContext.Database.CurrentTransaction ??
            throw new InvalidOperationException("Nesto.Domain events must be dispatched inside a transaction.");
        int handlerIndex = 0;

        foreach (object? handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            string handlerName = handler.GetType().FullName!;
            bool wasProcessed = await dbContext.ProcessedDomainEventHandlers.AnyAsync(
                processed => processed.EventId == context.EventId &&
                             processed.HandlerName == handlerName,
                cancellationToken);

            if (wasProcessed)
            {
                continue;
            }

            string savepoint = $"domain_event_handler_{handlerIndex++}";
            await transaction.CreateSavepointAsync(savepoint, cancellationToken);

            try
            {
                var handlerWrapper = HandlerWrapper.Create(handler, domainEventType);
                await handlerWrapper.Handle(domainEvent, context, cancellationToken);

                dbContext.ProcessedDomainEventHandlers.Add(
                    ProcessedDomainEventHandler.Create(context.EventId, handlerName, clock.UtcNow));

                await dbContext.SaveChangesAsync(cancellationToken);
                await transaction.ReleaseSavepointAsync(savepoint, cancellationToken);
            }
            catch
            {
                await transaction.RollbackToSavepointAsync(savepoint, cancellationToken);
                dbContext.ChangeTracker.Clear();
                throw;
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(
            IDomainEvent domainEvent,
            DomainEventContext context,
            CancellationToken cancellationToken);

        public static HandlerWrapper Create(object handler, Type domainEventType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                domainEventType,
                eventType => typeof(HandlerWrapper<>).MakeGenericType(eventType));

            return (HandlerWrapper)Activator.CreateInstance(wrapperType, handler);
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>)handler;

        public override Task Handle(
            IDomainEvent domainEvent,
            DomainEventContext context,
            CancellationToken cancellationToken) =>
            _handler.Handle((T)domainEvent, context, cancellationToken);
    }
}
