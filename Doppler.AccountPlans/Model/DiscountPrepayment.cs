namespace Doppler.AccountPlans.Model
{
    public class DiscountPrepayment
    {
        public decimal Amount { get; set; }
        public decimal NextAmount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public int MonthsToPay { get; set; }
    }
}
