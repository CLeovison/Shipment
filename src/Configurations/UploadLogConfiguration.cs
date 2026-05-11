using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shipment.Entities;

namespace Shipment.Configurations;

public sealed class UploadLogConfiguration : IEntityTypeConfiguration<UploadLog>
{
    public void Configure(EntityTypeBuilder<UploadLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Total)
            .HasDefaultValue(0);

        builder.Property(x => x.Succeeded)
            .HasDefaultValue(0);

        builder.Property(x => x.Failed)
            .HasDefaultValue(0);

        builder.Property(x => x.Processed)
            .HasDefaultValue(0);

        builder.Property(x => x.IsCompleted)
            .HasDefaultValue(false);

        builder.Property(x => x.StartedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.CompletedAt)
            .HasColumnType("timestamp with time zone");

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.IsCompleted);
    }
}
