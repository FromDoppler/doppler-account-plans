using System.Collections.Generic;

namespace Doppler.AccountPlans.Model
{
    public class PlanAmountDetails
    {
        public decimal DiscountPaymentAlreadyPaid { get; set; }
        public DiscountPrepayment DiscountPrepayment { get; set; }
        public IList<Tax> Taxes { get; set; }
        public decimal Total { get; set; }
    }
}
