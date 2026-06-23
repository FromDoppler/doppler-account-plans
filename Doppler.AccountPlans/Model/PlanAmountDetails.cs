using System;

namespace Doppler.AccountPlans.Model
{
    public class PlanAmountDetails
    {
        public decimal PlanFee { get; set; }
        public decimal DiscountPaymentAlreadyPaid { get; set; }
        public DiscountPrepayment DiscountPrepayment { get; set; }
        public decimal Total { get; set; }
        public decimal CurrentMonthTotal { get; set; }
        public DiscountPromocode DiscountPromocode { get; set; }
        public DiscountPlanFeeAdmin DiscountPlanFeeAdmin { get; set; }
        public decimal NextMonthTotal { get; set; }
        public decimal PositiveBalance { get; set; }
        public bool MajorThat21st { get; set; }
        public DateTime NextMonthDate { get; set; }
        public decimal? NextMonthTaxes { get; set; }
        public decimal? Taxes { get; set; }
        public decimal? CurrencyRate { get; set; }
        public DateTime? CurrencyDate { get; set; }
        public int TaxesPercentage { get; set; }
    }
}
