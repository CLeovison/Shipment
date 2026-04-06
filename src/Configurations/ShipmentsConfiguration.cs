using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentDetails>
{
    public void Configure(EntityTypeBuilder<ShipmentDetails> builder)
    {
        builder.HasKey(x => x.ShipmentId);

        builder.Property(x => x.PurchaseOrderNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.PurchaseOrderNumber)
            .IsUnique();

        builder.Property(x => x.Vendor)
            .IsRequired()
            .HasMaxLength(250);

        // Business date
        builder.Property(x => x.TimeOfArrival)
            .HasColumnType("date")
            .IsRequired();

        // System timestamps (UTC only)
        builder.Property(x => x.NotifyStartAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.LastNotifiedAt)
            .HasColumnType("timestamp with time zone");

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("NOW()");

        builder.Property(x => x.ModifiedAt);

        builder.Property(x => x.IsCompleted)
            .HasDefaultValue(false);

        builder.HasIndex(x => new
        {
            x.IsCompleted,
            x.LastNotifiedAt,
            x.TimeOfArrival
        });

        builder.HasOne(x => x.User)
            .WithMany(u => u.Shipments)
            .HasForeignKey(x => x.UserId);
    }
}