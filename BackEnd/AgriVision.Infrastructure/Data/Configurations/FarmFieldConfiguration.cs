using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class FarmFieldConfiguration : IEntityTypeConfiguration<FarmField>
{
    public void Configure(EntityTypeBuilder<FarmField> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(f => f.Crop)
               .HasMaxLength(100)
               .IsRequired();

        builder.Property(f => f.Status)
               .HasMaxLength(50)
               .HasDefaultValue("Healthy");

        builder.Property(f => f.SoilTemperature);

        builder.Property(f => f.SoilMoisture);
    }
}
