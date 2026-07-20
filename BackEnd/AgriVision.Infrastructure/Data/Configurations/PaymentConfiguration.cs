using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
               .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Currency)
               .HasMaxLength(10);

        builder.Property(p => p.Status)
               .HasMaxLength(50);

        builder.Property(p => p.Provider)
               .HasMaxLength(100);

        builder.Property(p => p.TransactionId)
               .HasMaxLength(200);

        builder.Property(p => p.StripeInvoiceId)
               .HasMaxLength(200);

        builder.Property(p => p.StripePaymentIntentId)
               .HasMaxLength(200);
    }
}
