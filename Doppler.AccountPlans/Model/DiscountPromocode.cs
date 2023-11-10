namespace Doppler.AccountPlans.Model
{
    public class DiscountPromocode
    {
        public decimal Amount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int ExtraCredits { get; set; }
        public int Duration { get; set; }
    }
}
