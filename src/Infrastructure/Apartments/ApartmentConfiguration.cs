using Nesto.Domain.Apartments;
using Nesto.Domain.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.Apartments;

internal sealed class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.ToTable("apartments");

        builder.HasKey(apartment => apartment.Id);

        builder.HasIndex(apartment => apartment.OwnerId);

        builder.Property(apartment => apartment.Name)
            .HasColumnName("name")
            .HasMaxLength(Name.MaxLength)
            .HasConversion(name => name.Value, value => Name.Create(value));

        builder.Property(apartment => apartment.Description)
            .HasColumnName("description")
            .HasMaxLength(Description.MaxLength)
            .HasConversion(description => description.Value, value => Description.Create(value));

        builder.OwnsOne(apartment => apartment.Address, address =>
        {
            address.Property(a => a.Country).HasColumnName("address_country").HasMaxLength(100);
            address.Property(a => a.State).HasColumnName("address_state").HasMaxLength(100);
            address.Property(a => a.ZipCode).HasColumnName("address_zip_code").HasMaxLength(20);
            address.Property(a => a.City).HasColumnName("address_city").HasMaxLength(100);
            address.Property(a => a.Street).HasColumnName("address_street").HasMaxLength(200);
        });

        builder.OwnsOne(apartment => apartment.Price, price =>
        {
            price.Property(money => money.Amount).HasColumnName("price_amount").HasPrecision(18, 2);
            price.Property(money => money.Currency)
                .HasColumnName("price_currency")
                .HasMaxLength(3)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.OwnsOne(apartment => apartment.CleaningFee, fee =>
        {
            fee.Property(money => money.Amount).HasColumnName("cleaning_fee_amount").HasPrecision(18, 2);
            fee.Property(money => money.Currency)
                .HasColumnName("cleaning_fee_currency")
                .HasMaxLength(3)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.Property<List<Amenity>>("_amenities")
            .HasColumnName("amenities")
            .HasColumnType("integer[]")
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Property(apartment => apartment.LastBookedOnUtc).HasColumnName("last_booked_on_utc");

        builder.Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
    }
}
