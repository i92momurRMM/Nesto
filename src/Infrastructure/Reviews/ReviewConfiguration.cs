using Nesto.Domain.Reviews;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.Reviews;

internal sealed class ReviewConfiguration : IEntityTypeConfiguration<Review>
{
    public void Configure(EntityTypeBuilder<Review> builder)
    {
        builder.ToTable("reviews");
        builder.HasKey(review => review.Id);
        builder.Property(review => review.Rating)
            .HasColumnName("rating")
            .HasConversion(rating => rating.Value, value => Rating.Create(value).Value);
        builder.Property(review => review.Comment)
            .HasColumnName("comment")
            .HasMaxLength(Comment.MaxLength)
            .HasConversion(comment => comment.Value, value => Comment.Create(value).Value);
        builder.HasIndex(review => review.BookingId).IsUnique();
        builder.HasIndex(review => review.ApartmentId);
        builder.HasIndex(review => review.UserId);
    }
}
