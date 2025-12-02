namespace Doppler.AccountPlans.Model
{
    public class Promotion
    {
        public int IdPromotion { get; set; }
        public int? ExtraCredits { get; set; }
        public int? DiscountPercentage { get; set; }
        public int? Duration { get; set; }
        public string Code { get; set; }
        public bool Active { get; set; }
        public int? IdAddOnPlan { get; set; }
    }
}
