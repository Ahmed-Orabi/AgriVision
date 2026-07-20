using AgriVision.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgriVision.Infrastructure.Data.Configurations
{
    public class InsectDetectionConfiguration : IEntityTypeConfiguration<InsectDetection>
    {
        public void Configure(EntityTypeBuilder<InsectDetection> builder)
        {
            builder.HasKey(i => i.InsectDetectionId);
            builder.HasOne(i => i.User)
                .WithMany(u => u.InsectDetections)
                .HasForeignKey(i => i.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
