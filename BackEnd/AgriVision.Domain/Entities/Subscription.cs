using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Domain.Enums;

namespace AgriVision.Domain.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; } // Primary key for the subscription

        public string UserId { get; set; } = default!; // Subscribed user ID (FK)

        public ApplicationUser User { get; set; } = default!; // Subscribed user

        public SubscriptionPlan Plan { get; set; } // Subscription plan type

        public DateTime StartDate { get; set; } // Subscription start date

        public DateTime? EndDate { get; set; } // Subscription end date

        public bool IsActive { get; set; } // Subscription status

        public decimal Price { get; set; } // Subscription price

        public string? StripeCustomerId { get; set; } // Stripe customer id

        public string? StripeSubscriptionId { get; set; } // Stripe subscription id

        public string? StripePriceId { get; set; } // Stripe price id

        public DateTime? CurrentPeriodEnd { get; set; } // Stripe current period end

        public string? Status { get; set; } // Stripe status
    }
}
