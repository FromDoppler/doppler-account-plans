using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;

namespace Doppler.AccountPlans.RenewalHandlers
{
    public class MonthlyHandler : RenewalHandler
    {
        public MonthlyHandler(IDateTimeProvider dateTimeProvider) : base(dateTimeProvider) { }

        public override PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan)
        {
            var dateNow = DateTimeProvider.Now;

            var discountPaymentAlreadyPaid = dateNow.Day >= 21 ? 0 : newPlan.Fee - currentPlan.Fee;

            return new PlanAmountDetails
            {
                Total = newPlan.Fee - (((newPlan.Fee * newDiscount.DiscountPlanFee) / 100)) - discountPaymentAlreadyPaid,
                DiscountPaymentAlreadyPaid = discountPaymentAlreadyPaid,
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = (newPlan.Fee * newDiscount.DiscountPlanFee) / 100,
                    DiscountPercentage = newDiscount.DiscountPlanFee
                }
            };
        }
    }
}
