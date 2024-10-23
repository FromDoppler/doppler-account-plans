using Doppler.AccountPlans.Model;
using System;

namespace Doppler.AccountPlans.Helpers
{
    public interface ICalculateAmountDetalisHelper
    {
        PlanAmountDetails CalculateAmountDetails(PlanInformation newPlan, ref PlanDiscountInformation newDiscount, ref UserPlanInformation currentPlan, DateTime now, Promotion promotion, TimesApplyedPromocode timesAppliedPromocode, Promotion currentPromotion, DateTime? firstUpgradeDate, PlanDiscountInformation currentDiscountPlan, decimal creditsDiscount);
    }
}
