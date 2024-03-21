using Doppler.AccountPlans.Model;
using System;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Helpers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Doppler.AccountPlans.Utils
{
    public static class CalculateUpgradeCostHelper
    {
        public static PlanAmountDetails CalculatePlanAmountDetails(
            PlanInformation newPlan,
            PlanDiscountInformation newDiscount,
            UserPlanInformation currentPlan,
            DateTime now, Promotion promotion,
            TimesApplyedPromocode timesAppliedPromocode,
            Promotion currentPromotion,
            UserPlanInformation firstUpgrade,
            PlanDiscountInformation currentDiscountPlan,
            decimal creditsDiscount,
            PlanTypeEnum planType)
        {
            return GetHelper(planType).CalculateAmountDetails(newPlan, ref newDiscount, ref currentPlan, now, promotion, timesAppliedPromocode, currentPromotion, firstUpgrade, currentDiscountPlan, creditsDiscount);
        }

        public static decimal CalculateLandingPlanAmountDetails(
            List<LandingPlanSummary> landingPlansSummary,
            IEnumerable<LandingPlanInformation> landingsPlanInformation,
            PlanDiscountInformation newDiscount)
        {
            decimal totalFee = 0;

            foreach (LandingPlanSummary landingPlanSummary in landingPlansSummary)
            {
                var landingPlan = landingsPlanInformation.FirstOrDefault(x => x.PlanId == landingPlanSummary.IdLandingPlan);

                if (landingPlan is not null)
                {
                    totalFee += landingPlan.Fee * landingPlanSummary.NumberOfPlans;
                }
            }

            //Usar el newDiscount

            return totalFee;
        }

        private static ICalculateAmountDetalisHelper GetHelper(PlanTypeEnum planType)
        {
            switch (planType)
            {
                case PlanTypeEnum.Marketing:
                    return new MarketingPlan();
                case PlanTypeEnum.Chat:
                    return new ChatPlan();
                default:
                    return null;
            }
        }
    }
}
