using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName)
               .HasMaxLength(200);

        builder.Property(u => u.SubscriptionPlan)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(u => u.LastLoginAtUtc);

        builder.HasMany(u => u.Farms)
               .WithOne(f => f.Owner)
               .HasForeignKey(f => f.OwnerId);

        builder.HasMany(u => u.Subscriptions)
               .WithOne(s => s.User)
               .HasForeignKey(s => s.UserId);

        builder.HasMany(u => u.Payments)
               .WithOne(p => p.User)
               .HasForeignKey(p => p.UserId);
    }
}
