using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipmennt.Entities;

namespace Shipment.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.HasKey(x => x.PermissionId);

        builder.Property(x => x.PermissionName)
               .IsRequired();

        builder.Property(x => x.Description)
               .IsRequired();
    }
}