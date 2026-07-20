using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Plan)
               .HasConversion<int>()
               .IsRequired();

        builder.Property(s => s.Price)
               .HasColumnType("decimal(18,2)");

        builder.Property(s => s.StripeCustomerId)
               .HasMaxLength(200);

        builder.Property(s => s.StripeSubscriptionId)
               .HasMaxLength(200);

        builder.Property(s => s.StripePriceId)
               .HasMaxLength(200);

        builder.Property(s => s.Status)
               .HasMaxLength(100);
    }
}
