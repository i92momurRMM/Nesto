using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.DomainEvents;

internal sealed class ProcessedDomainEventHandlerConfiguration
    : IEntityTypeConfiguration<ProcessedDomainEventHandler>
{
    public void Configure(EntityTypeBuilder<ProcessedDomainEventHandler> builder)
    {
        builder.ToTable("processed_domain_event_handlers");
        builder.HasKey(processed => new { processed.EventId, processed.HandlerName });
        builder.Property(processed => processed.HandlerName).HasMaxLength(500);
    }
}
