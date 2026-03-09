using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class TokenConiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.RefreshTokenId);

        builder.HasIndex(x => x.Token)
        .IsUnique();

        builder.Property(x => x.RefreshTokenId)
        .HasDefaultValueSql("gen_random_uuid()")
        .ValueGeneratedOnAdd();

        builder.Property(x => x.Token)
        .HasMaxLength(200)
        .IsRequired();

        builder.Property(x => x.CreatedAt)
        .HasDefaultValueSql("current_date");

        builder.Property(x => x.ExpiresAt)
        .HasDefaultValueSql("current_date");

        builder.HasOne(x => x.User)
        .WithMany()
        .HasForeignKey(x => x.UserId);
    }
}