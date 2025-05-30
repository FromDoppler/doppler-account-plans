using Doppler.AccountPlans.Enums;

namespace Doppler.AccountPlans.Model
{
    public class BasePlanInformation
    {
        public int PlanId { set; get; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public decimal Fee { get; set; }
        public decimal Additional { get; set; }
        public PlanTypeEnum PlanType { get; set; }
    }
}
