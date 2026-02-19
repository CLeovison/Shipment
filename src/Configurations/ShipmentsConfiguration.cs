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
               .HasMaxLength(250);

        builder.Property(x => x.Vendor)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.TimeOfArrival)
               .IsRequired()
               .HasColumnType("date");

        builder.Property(x => x.CreatedAt)
               .HasDefaultValueSql("CURRENT_DATE")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.UpdatedAt)
               .HasDefaultValueSql("CURRENT_DATE")
               .ValueGeneratedOnUpdate();
    }
}