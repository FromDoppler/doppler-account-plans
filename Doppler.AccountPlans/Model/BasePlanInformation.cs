using Doppler.AccountPlans.Enums;

namespace Doppler.AccountPlans.Model
{
    public class BasePlanInformation
    {
        public int PlanId { set; get; }
        public PlanTypeEnum PlanType { get; set; }
    }
}
