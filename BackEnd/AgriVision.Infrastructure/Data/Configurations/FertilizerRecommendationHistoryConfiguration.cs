using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations
{
    public class FertilizerRecommendationHistoryConfiguration
        : IEntityTypeConfiguration<FertilizerRecommendationHistory>
    {
        public void Configure(EntityTypeBuilder<FertilizerRecommendationHistory> builder)
        {
            // Table
            builder.ToTable("FertilizerRecommendationHistories");

            // Key
            builder.HasKey(x => x.Id);

            // User
            builder.Property(x => x.UserId)
                   .IsRequired()
                   .HasMaxLength(450);

            builder.HasIndex(x => x.UserId);

            // Input values
            builder.Property(x => x.Temperature).IsRequired();
            builder.Property(x => x.Moisture).IsRequired();
            builder.Property(x => x.Rainfall).IsRequired();
            builder.Property(x => x.PH).IsRequired();

            builder.Property(x => x.Nitrogen).IsRequired();
            builder.Property(x => x.Phosphorous).IsRequired();
            builder.Property(x => x.Potassium).IsRequired();
            builder.Property(x => x.Carbon).IsRequired();

            builder.Property(x => x.Soil)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Crop)
                   .IsRequired()
                   .HasMaxLength(100);

            // Output (Top 3 Fertilizers)
            builder.Property(x => x.Fertilizer1)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(x => x.Confidence1)
                   .IsRequired();

            builder.Property(x => x.Fertilizer2)
                   .HasMaxLength(100);

            builder.Property(x => x.Confidence2);

            builder.Property(x => x.Fertilizer3)
                   .HasMaxLength(100);

            builder.Property(x => x.Confidence3);

            // Audit
            builder.Property(x => x.CreatedAtUtc)
                   .IsRequired();
        }
    }
}
