using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class SensorConfiguration : IEntityTypeConfiguration<Sensor>
{
    public void Configure(EntityTypeBuilder<Sensor> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
               .HasMaxLength(200)
               .IsRequired();

        builder.Property(s => s.Type)
               .HasConversion<int>()
               .IsRequired();

        builder.HasMany(s => s.Readings)
               .WithOne(r => r.Sensor)
               .HasForeignKey(r => r.SensorId);
    }
}