using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipmennt.Entities;

namespace Shipment.Configurations;


public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(x => x.RoleId);

        builder.Property(x => x.RoleName)
               .IsRequired();

        builder.Property(x => x.Description)
               .IsRequired();
    }
}