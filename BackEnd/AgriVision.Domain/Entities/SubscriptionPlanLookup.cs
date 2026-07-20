using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AgriVision.Domain.Enums;

namespace AgriVision.Domain.Entities
{
    public class SubscriptionPlanLookup
    {
        public int Id { get; set; } // Primary key for the plan
        public SubscriptionPlan Plan { get; set; } // Plan type (Enum)
        public string Name { get; set; } = default!; // Plan name 
        public decimal Price { get; set; } // Plan price
    }
}
