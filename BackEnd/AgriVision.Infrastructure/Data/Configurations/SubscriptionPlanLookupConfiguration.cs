using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class SubscriptionPlanLookupConfiguration : IEntityTypeConfiguration<SubscriptionPlanLookup>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlanLookup> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Plan)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(p => p.Name)
               .HasMaxLength(50)
               .IsRequired();

        builder.Property(p => p.Price)
               .HasColumnType("decimal(18,2)");
    }
}