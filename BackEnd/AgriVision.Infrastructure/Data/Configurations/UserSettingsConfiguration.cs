using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class UserSettingsConfiguration : IEntityTypeConfiguration<UserSettings>
{
    public void Configure(EntityTypeBuilder<UserSettings> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MaintenanceWindow)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.NotificationsEnabled)
            .IsRequired();

        builder.Property(x => x.RobotAutoMode)
            .IsRequired();

        builder.Property(x => x.RobotFirmwareUpdates)
            .IsRequired();

        builder.Property(x => x.RobotSafetyLock)
            .IsRequired();

        builder.Property(x => x.RobotEmergencyAlerts)
            .IsRequired();

        builder.Property(x => x.Country)
            .HasMaxLength(120);

        builder.Property(x => x.City)
            .HasMaxLength(120);

        builder.Property(x => x.AddressLine)
            .HasMaxLength(300);

        builder.HasOne(x => x.User)
            .WithOne(u => u.Settings)
            .HasForeignKey<UserSettings>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId)
            .IsUnique();
    }
}
