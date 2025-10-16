using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using System;

namespace Doppler.AccountPlans.Helpers
{
    public interface ICalculateAmountDetalisHelper
    {
        PlanAmountDetails CalculateAmountDetails(PlanTypeEnum newPlanType, PlanInformation newPlan, ref PlanDiscountInformation newDiscount, ref UserPlan currentPlan, DateTime now, Promotion promotion, TimesApplyedPromocode timesAppliedPromocode, Promotion currentPromotion, DateTime? firstUpgradeDate, PlanDiscountInformation currentDiscountPlan, decimal creditsDiscount);
    }
}
