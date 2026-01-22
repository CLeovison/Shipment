using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<ShipmentDetails>
{
    public void Configure(EntityTypeBuilder<ShipmentDetails> builder)
    {
        
    }
}