using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentDetails>
{
    public void Configure(EntityTypeBuilder<ShipmentDetails> builder)
    {
        // Primary Key
        builder.HasKey(x => x.ShipmentId);

        builder.Property(x => x.ShipmentId)
               .ValueGeneratedOnAdd();

        // Required Fields
        builder.Property(x => x.PurchaseOrderNumber)
               .IsRequired()
               .HasMaxLength(100);

        builder.HasIndex(x => x.PurchaseOrderNumber)
               .IsUnique();

        builder.Property(x => x.Vendor)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.TimeOfArrival)
               .IsRequired()
               .HasColumnType("date");

        // Timestamps
        builder.Property(x => x.CreatedAt)
               .HasDefaultValueSql("NOW()")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.ModifiedAt)
               .IsRequired(false);

        // NEW: Daily Reminder Fields
        builder.Property(x => x.NotifyStartAt)
               .IsRequired();

        builder.Property(x => x.LastNotifiedAt)
               .IsRequired(false);

        builder.Property(x => x.IsCompleted)
               .HasDefaultValue(false);

        // Indexes for efficient worker queries
        builder.HasIndex(x => new
        {
            x.NotifyStartAt,
            x.TimeOfArrival,
            x.LastNotifiedAt,
            x.IsCompleted
        });

        builder.HasIndex(x => x.TimeOfArrival);

        // Navigation Property
        builder.HasOne(x => x.User)
               .WithMany(u => u.Shipments)
               .HasForeignKey(x => x.UserId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}