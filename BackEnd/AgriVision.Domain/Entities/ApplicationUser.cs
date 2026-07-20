using System;
using System.Collections.Generic;
using AgriVision.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace AgriVision.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; } // User Full Name

        public SubscriptionPlan SubscriptionPlan { get; set; } // User Subscription Plan

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Creation Date of User Account 

        public DateTime? LastLoginAtUtc { get; set; } // Last login timestamp

        public ICollection<Farm> Farms { get; set; } = new List<Farm>(); // User Farms Own

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>(); // User Subscription History

        public ICollection<Payment> Payments { get; set; } = new List<Payment>(); // User Payments History

        public UserSettings? Settings { get; set; } // User preferences/settings

        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public ICollection<InsectDetection> InsectDetections { get; set; } = new List<InsectDetection>();

        public ICollection<PlantDetection> PlantDetections { get; set; } = new List<PlantDetection>();
    }
}
