using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgriVision.Domain.Entities
{
    public class Payment
    {
        public Guid Id { get; set; } // Primary key for the payment

        public string UserId { get; set; } = default!; // user Id who made payment

        public ApplicationUser User { get; set; } = default!; // Paying user

        public Guid? SubscriptionId { get; set; } // Subscription linked to the payment (FK)

        public Subscription? Subscription { get; set; } // Linked subscription

        public decimal Amount { get; set; } // Payment amount

        public string Currency { get; set; } = "USD"; // Currency used

        public string Status { get; set; } = "Pending"; // Payment status

        public string? Provider { get; set; } // Payment provider

        public string? TransactionId { get; set; } // Transaction ID in the payment provider

        public string? StripeInvoiceId { get; set; } // Stripe invoice id

        public string? StripePaymentIntentId { get; set; } // Stripe payment intent id

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Payment creation timestamp
    }
}
