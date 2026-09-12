using Nesto.Domain.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nesto.Infrastructure.Users;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(refreshToken => refreshToken.Id);

        builder.Property(refreshToken => refreshToken.Token).HasColumnName("token").HasMaxLength(200);

        builder.Property(refreshToken => refreshToken.UserId).HasColumnName("user_id");

        builder.Property(refreshToken => refreshToken.ExpiresOnUtc).HasColumnName("expires_on_utc");

        builder.HasIndex(refreshToken => refreshToken.Token).IsUnique();
    }
}
