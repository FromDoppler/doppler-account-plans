namespace Doppler.AccountPlans.Model
{
    public class PlanAmountDetails
    {
        public decimal DiscountPaymentAlreadyPaid { get; set; }
        public DiscountPrepayment DiscountPrepayment { get; set; }
        public decimal Total { get; set; }
        public DiscountPromocode DiscountPromocode { get; set; }
    }
}
