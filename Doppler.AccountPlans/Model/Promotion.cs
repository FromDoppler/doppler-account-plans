using Doppler.AccountPlans.Enums;
using System;

namespace Doppler.AccountPlans.Model
{
    public class Promotion
    {
        public int IdPromotion { get; set; }
        public int? IdUserTypePlan { get; set; }
        public UserTypesEnum? IdUserType { get; set; }
        public int? ExtraCredits { get; set; }
        public int? DiscountPercentage { get; set; }
        public int? Duration { get; set; }
        public string Code { get; set; }
        public bool AllPlans { get; set; }
        public bool AllSubscriberPlans { get; set; }
        public bool AllPrepaidPlans { get; set; }
        public bool AllMonthlyPlans { get; set; }
        public bool Active { get; set; }
        public int? IdAddOnPlan { get; set; }
        public int? IdAddOnType { get; set; }
        public string Quantity { get; set; }
        public bool CanApply { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
