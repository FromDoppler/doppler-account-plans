using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;

namespace Doppler.AccountPlans.RenewalHandlers
{
    public abstract class RenewalHandler
    {
        protected readonly IDateTimeProvider DateTimeProvider;

        protected RenewalHandler(IDateTimeProvider dateTimeProvider)
        {
            DateTimeProvider = dateTimeProvider;
        }

        public abstract PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan);
    }
}
