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

        builder.HasOne(x => x.User)
        .WithMany(x => x.UserRoles)
        .HasForeignKey(x => x.UserId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Roles)
        .WithMany(r => r.UserRoles)
        .HasForeignKey(x => x.RoleId)
        .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.RoleId);
    }
}