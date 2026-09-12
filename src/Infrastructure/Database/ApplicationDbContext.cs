using Nesto.Application.Abstractions.Data;
using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Reviews;
using Nesto.Domain.Users;
using Nesto.Infrastructure.DomainEvents;
using Nesto.Infrastructure.Identity;
using Nesto.Infrastructure.IntegrationEvents;
using Nesto.Infrastructure.Outbox;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nesto.SharedKernel;

namespace Nesto.Infrastructure.Database;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityAccount, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public new DbSet<User> Users { get; set; }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<Apartment> Apartments { get; set; }

    public DbSet<Booking> Bookings { get; set; }

    public DbSet<Review> Reviews { get; set; }

    public DbSet<OutboxMessage> OutboxMessages { get; set; }

    public DbSet<ProcessedDomainEventHandler> ProcessedDomainEventHandlers { get; set; }

    public DbSet<IntegrationOutboxMessage> IntegrationOutboxMessages { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        builder.HasDefaultSchema(Schemas.Default);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<IDomainEvent> domainEvents = ExtractDomainEvents();
        DateTime occurredOnUtc = DateTime.UtcNow;

        OutboxMessages.AddRange(domainEvents.Select(domainEvent => OutboxMessage.Create(domainEvent, occurredOnUtc)));

        try
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyException("Otra transaccion modifico los mismos datos.", exception);
        }
    }

    private List<IDomainEvent> ExtractDomainEvents() =>
        [.. ChangeTracker
            .Entries<Entity>()
            .Select(entry => entry.Entity)
            .SelectMany(entity =>
            {
                List<IDomainEvent> domainEvents = [.. entity.DomainEvents];

                entity.ClearDomainEvents();

                return domainEvents;
            })];
}
