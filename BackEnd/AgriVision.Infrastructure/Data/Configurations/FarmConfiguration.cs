using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class FarmConfiguration : IEntityTypeConfiguration<Farm>
{
    public void Configure(EntityTypeBuilder<Farm> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(f => f.Address)
               .HasMaxLength(300);

        builder.Property(f => f.Description)
               .HasMaxLength(1000);

        builder.Property(f => f.SoilType)
               .HasMaxLength(100);

        builder.Property(f => f.Length)
               .IsRequired();

        builder.Property(f => f.Width)
               .IsRequired();

        builder.Property(f => f.LengthUnit)
               .HasMaxLength(20)
               .HasDefaultValue("m");

        builder.Property(f => f.Temperature);

        builder.Property(f => f.Humidity);

        builder.OwnsOne(f => f.Location, loc =>
        {
            loc.Property(l => l.Latitude).HasColumnName("Latitude");
            loc.Property(l => l.Longitude).HasColumnName("Longitude");
        });

        builder.HasMany(f => f.Fields)
               .WithOne(ff => ff.Farm)
               .HasForeignKey(ff => ff.FarmId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
