using AgriVision.Domain.Enums;

namespace AgriVision.Application.Subscriptions
{
    public sealed record SubscriptionPlanLimits(
        int FarmLimit,
        int CropMonthlyLimit,
        int FertilizerMonthlyLimit,
        int DiseaseMonthlyLimit,
        bool DigitalTwinEnabled);

    public static class SubscriptionPlanRules
    {
        public static SubscriptionPlanLimits GetLimits(SubscriptionPlan plan)
        {
            return plan switch
            {
                SubscriptionPlan.Free => new SubscriptionPlanLimits(
                    FarmLimit: 1,
                    CropMonthlyLimit: 3,
                    FertilizerMonthlyLimit: 3,
                    DiseaseMonthlyLimit: 3,
                    DigitalTwinEnabled: false),
                SubscriptionPlan.Basic => new SubscriptionPlanLimits(
                    FarmLimit: 3,
                    CropMonthlyLimit: 10,
                    FertilizerMonthlyLimit: 10,
                    DiseaseMonthlyLimit: 10,
                    DigitalTwinEnabled: true),
                SubscriptionPlan.Advanced => new SubscriptionPlanLimits(
                    FarmLimit: 10,
                    CropMonthlyLimit: 0,
                    FertilizerMonthlyLimit: 0,
                    DiseaseMonthlyLimit: 0,
                    DigitalTwinEnabled: true),
                _ => new SubscriptionPlanLimits(1, 3, 3, 3, false)
            };
        }

        public static int GetTotalAiLimit(SubscriptionPlanLimits limits)
        {
            if (limits.CropMonthlyLimit <= 0
                || limits.FertilizerMonthlyLimit <= 0
                || limits.DiseaseMonthlyLimit <= 0)
            {
                return 0;
            }

            return limits.CropMonthlyLimit + limits.FertilizerMonthlyLimit + limits.DiseaseMonthlyLimit;
        }
    }
}
