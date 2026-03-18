using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentDetails>
{
       public void Configure(EntityTypeBuilder<ShipmentDetails> builder)
       {
              builder.HasKey(x => x.ShipmentId);

              builder.Property(x => x.ShipmentId)
                     .ValueGeneratedOnAdd();

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

              builder.Property(x => x.CreatedAt)
                     .HasDefaultValueSql("NOW()")
                     .ValueGeneratedOnAdd();

              builder.Property(x => x.ModifiedAt)
                     .IsRequired(false);

              builder.Property(x => x.IsNotified)
                     .HasDefaultValue(false);

              builder.HasIndex(x => x.TimeOfArrival);

              builder.HasIndex(x => new
              {
                     x.TimeOfArrival,
                     x.IsNotified
              });

              builder.HasOne(x => x.User)
                     .WithMany(u => u.Shipments)
                     .HasForeignKey(x => x.UserId)
                     .OnDelete(DeleteBehavior.Cascade);
       }
}