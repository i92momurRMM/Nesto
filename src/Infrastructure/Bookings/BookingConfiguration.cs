using Nesto.Domain.Apartments;
using Nesto.Domain.Bookings;
using Nesto.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.Bookings;

internal sealed class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("bookings");

        builder.HasKey(booking => booking.Id);

        builder.Property(booking => booking.ApartmentId).HasColumnName("apartment_id");
        builder.Property(booking => booking.UserId).HasColumnName("user_id");
        builder.Property(booking => booking.Status).HasColumnName("status").HasConversion<int>();
        builder.Property(booking => booking.CreatedOnUtc).HasColumnName("created_on_utc");
        builder.Property(booking => booking.ConfirmedOnUtc).HasColumnName("confirmed_on_utc");
        builder.Property(booking => booking.RejectedOnUtc).HasColumnName("rejected_on_utc");
        builder.Property(booking => booking.CompletedOnUtc).HasColumnName("completed_on_utc");
        builder.Property(booking => booking.CancelledOnUtc).HasColumnName("cancelled_on_utc");

        builder.OwnsOne(booking => booking.Duration, duration =>
        {
            duration.Property(d => d.Start).HasColumnName("duration_start");
            duration.Property(d => d.End).HasColumnName("duration_end");
        });

        ConfigureMoney(builder, booking => booking.PriceForPeriod, "price");
        ConfigureMoney(builder, booking => booking.CleaningFee, "cleaning_fee");
        ConfigureMoney(builder, booking => booking.AmenitiesUpCharge, "amenities_up_charge");
        ConfigureMoney(builder, booking => booking.TotalPrice, "total_price");

        builder.HasOne<Apartment>()
            .WithMany()
            .HasForeignKey(booking => booking.ApartmentId);

        builder.HasIndex(booking => booking.ApartmentId);

        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }

    private static void ConfigureMoney(
        EntityTypeBuilder<Booking> builder,
        System.Linq.Expressions.Expression<Func<Booking, Money?>> selector,
        string columnPrefix) =>
        builder.OwnsOne(selector, money =>
        {
            money.Property(m => m.Amount).HasColumnName($"{columnPrefix}_amount").HasPrecision(18, 2);
            money.Property(m => m.Currency)
                .HasColumnName($"{columnPrefix}_currency")
                .HasMaxLength(3)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });
}
