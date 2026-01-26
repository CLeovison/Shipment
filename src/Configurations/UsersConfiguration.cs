using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<Users>
{
    public void Configure(EntityTypeBuilder<Users> builder)
    {
        builder.HasKey(x => x.UserId);


        builder.Property(x => x.UserId)
               .ValueGeneratedOnAdd();

        builder.Property(x => x.FirstName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.LastName)
               .IsRequired()
               .HasMaxLength(50);

        builder.Property(x => x.Username)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.Password)
               .IsRequired()
               .HasMaxLength(250);

        builder.Property(x => x.Birthday)
               .IsRequired()
               .HasColumnType("date");

        builder.Property(x => x.CreatedAt)
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .ValueGeneratedOnAdd();

        builder.Property(x => x.UpdatedAt)
               .HasDefaultValueSql("CURRENT_TIMESTAMP")
               .ValueGeneratedOnUpdate();

        builder.HasIndex(x => x.Username)
               .IsUnique();
    }
}