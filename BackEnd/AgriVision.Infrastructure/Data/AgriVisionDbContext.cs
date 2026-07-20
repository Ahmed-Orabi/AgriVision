using AgriVision.Domain.Entities;
using AgriVision.Domain.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AgriVision.Infrastructure.Data
{
    public class AgriVisionDbContext : IdentityDbContext<ApplicationUser>
    {
        public AgriVisionDbContext(DbContextOptions<AgriVisionDbContext> options)
            : base(options)
        {
        }

        // Users owned farms
        public DbSet<Farm> Farms { get; set; } = default!;

        // Farm zones / fields
        public DbSet<FarmField> FarmFields { get; set; } = default!;

        // Sensors installed in farms
        public DbSet<Sensor> Sensors { get; set; } = default!;

        // Sensor readings history
        public DbSet<SensorReading> SensorReadings { get; set; } = default!;

        // User subscriptions
        public DbSet<Subscription> Subscriptions { get; set; } = default!;

        // User payments
        public DbSet<Payment> Payments { get; set; } = default!;

        // Subscription plans lookup
        public DbSet<SubscriptionPlanLookup> SubscriptionPlanLookups { get; set; } = default!;

        // Crop Recommendation History
        public DbSet<CropRecommendationHistory> CropRecommendationHistories { get; set; } = default!;
        
        // Fertilizer Recommendation History
        public DbSet<FertilizerRecommendationHistory> FertilizerRecommendationHistories { get; set; }

        // User settings/preferences
        public DbSet<UserSettings> UserSettings { get; set; } = default!;
        public DbSet<Notification> Notifications { get; set; } = default!;
        public DbSet<InsectDetection> InsectDetections { get; set; } = default!;
        public DbSet<PlantDetection> PlantDetections { get; set; } = default!;
        // Irrigation Recommendation History
        public DbSet<IrrigationRecommendationHistory> IrrigationRecommendationHistories { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Apply all configurations in Infrastructure assembly (Configurations folder)
            builder.ApplyConfigurationsFromAssembly(typeof(AgriVisionDbContext).Assembly);

            // Seed Roles
            builder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "7C7F1A7B-9F2F-4E61-8B1A-1A8D2F3A9A11",
                    Name = "Admin",
                    NormalizedName = "ADMIN",
                    ConcurrencyStamp = "F1C1B9B9-7A75-4B2A-B5E2-0B5E9AF5C311"
                },
                new IdentityRole
                {
                    Id = "1B2D3C4E-5F67-4890-ABCD-1234567890AB",
                    Name = "Farmer",
                    NormalizedName = "FARMER",
                    ConcurrencyStamp = "E0B2F8D1-1CC7-4A63-9DBD-4E3B2B39B889"
                }
            );

            // Seed Subscription Plans Lookup
            builder.Entity<SubscriptionPlanLookup>().HasData(
                new SubscriptionPlanLookup { Id = 1, Plan = SubscriptionPlan.Free, Name = "Free", Price = 0m },
                new SubscriptionPlanLookup { Id = 2, Plan = SubscriptionPlan.Basic, Name = "Basic", Price = 3.99m },
                new SubscriptionPlanLookup { Id = 3, Plan = SubscriptionPlan.Advanced, Name = "Advanced", Price = 9.99m }
            );
        }
    }
}
