using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;


public sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRole");


        builder.HasKey(x => new
        {
            x.RoleId,
            x.UserId
        });

        builder.HasOne<Users>().WithMany().HasForeignKey(x => x.UserId)
    }
}