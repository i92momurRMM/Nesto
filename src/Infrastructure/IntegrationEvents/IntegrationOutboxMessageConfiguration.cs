using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.IntegrationEvents;

internal sealed class IntegrationOutboxMessageConfiguration : IEntityTypeConfiguration<IntegrationOutboxMessage>
{
    public void Configure(EntityTypeBuilder<IntegrationOutboxMessage> builder)
    {
        builder.ToTable("integration_outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Type).HasMaxLength(500);
        builder.Property(message => message.Content).HasColumnType("jsonb");
        builder.Property(message => message.Error).HasMaxLength(4000);
        builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc });
    }
}
