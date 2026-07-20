using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations
{
    public class PlantDetectionConfiguration : IEntityTypeConfiguration<PlantDetection>
    {
        public void Configure(EntityTypeBuilder<PlantDetection> builder)
        {
            builder.HasKey(pd => pd.PlantDetectionId);
            builder.HasOne(pd => pd.User)
                .WithMany(u => u.PlantDetections)
                .HasForeignKey(pd => pd.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
