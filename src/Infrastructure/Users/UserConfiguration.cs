using Nesto.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.Users;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);

        builder.Property(user => user.Email)
            .HasColumnName("email")
            .HasMaxLength(Email.MaxLength)
            .HasConversion(email => email.Value, value => Email.Create(value));

        builder.Property(user => user.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(PersonName.MaxLength)
            .HasConversion(name => name.Value, value => PersonName.Create(value));

        builder.Property(user => user.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(PersonName.MaxLength)
            .HasConversion(name => name.Value, value => PersonName.Create(value));

        builder.HasIndex(user => user.Email).IsUnique();

        builder.HasMany(user => user.RefreshTokens)
            .WithOne(refreshToken => refreshToken.User)
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
