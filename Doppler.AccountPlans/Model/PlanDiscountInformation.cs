
namespace Doppler.AccountPlans.Model
{
    public class PlanDiscountInformation
    {
        public string IdDiscountPlan { get; set; }
        public decimal DiscountPlanFee { get; set; }
        public int MonthPlan { get; set; }
        public bool ApplyPromo { get; set; }
    }
}
