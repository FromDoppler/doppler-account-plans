using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;

namespace Doppler.AccountPlans.RenewalHandlers
{
    public class BiannualHandler : RenewalHandler
    {
        public BiannualHandler(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

        public override PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan)
        {
            return new PlanAmountDetails()
            {
                Total = 0,
                DiscountPaymentAlreadyPaid = 0,
                DiscountPrepayment = new DiscountPrepayment
                {
                    DiscountPercentage = 0,
                    Amount = 0
                }
            };
        }
    }
}
