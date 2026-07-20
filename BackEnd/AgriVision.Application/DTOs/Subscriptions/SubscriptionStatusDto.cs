using System;

namespace AgriVision.Application.DTOs.Subscriptions
{
    public class SubscriptionStatusDto
    {
        public string Plan { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public bool IsActive { get; set; }
        public int FarmsUsed { get; set; }
        public int FarmsLimit { get; set; }
        public int SensorsUsed { get; set; }
        public int SensorsLimit { get; set; }
        public int AiRequestsUsed { get; set; }
        public int AiRequestsLimit { get; set; }
        public int CropRequestsUsed { get; set; }
        public int CropRequestsLimit { get; set; }
        public int FertilizerRequestsUsed { get; set; }
        public int FertilizerRequestsLimit { get; set; }
        public int DiseaseRequestsUsed { get; set; }
        public int DiseaseRequestsLimit { get; set; }
        public bool DigitalTwinEnabled { get; set; }
    }
}
