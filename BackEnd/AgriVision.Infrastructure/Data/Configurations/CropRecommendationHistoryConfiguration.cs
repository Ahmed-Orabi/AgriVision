using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using AgriVision.Domain.Entities;

namespace AgriVision.Infrastructure.Data.Configurations
{
    public class CropRecommendationHistoryConfiguration : IEntityTypeConfiguration<CropRecommendationHistory>
    {
        public void Configure(EntityTypeBuilder<CropRecommendationHistory> builder)
        {
            builder.ToTable("CropRecommendationHistories");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.UserId)
                .IsRequired()
                .HasMaxLength(450);

            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Property(x => x.Crop1).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Crop2).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Crop3).IsRequired().HasMaxLength(100);

            builder.Property(x => x.CreatedAtUtc).IsRequired();

            // Useful index
            builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        }
    }
}
