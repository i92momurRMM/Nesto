using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.Outbox;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.Type).HasMaxLength(500);
        builder.Property(message => message.Content).HasColumnType("jsonb");
        builder.Property(message => message.Error).HasMaxLength(4000);
        builder.HasIndex(message => new { message.ProcessedOnUtc, message.OccurredOnUtc });
    }
}
