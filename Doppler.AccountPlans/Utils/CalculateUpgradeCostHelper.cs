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

        public static PlanAmountDetails CalculateLandingPlanAmountDetails(
            UserPlanInformation currentPlan,
            DateTime now,
            List<LandingPlanSummary> landingPlansSummary,
            IEnumerable<LandingPlanInformation> landingsPlanInformation,
            PlanDiscountInformation discount,
            UserPlanInformation lastLandingPlan)
        {
            var result = new PlanAmountDetails { DiscountPromocode = null };
            decimal baseLandingPlansFee = 0;

            foreach (LandingPlanSummary landingPlanSummary in landingPlansSummary)
            {
                var landingPlan = landingsPlanInformation.FirstOrDefault(x => x.PlanId == landingPlanSummary.IdLandingPlan);

                if (landingPlan is not null)
                {
                    baseLandingPlansFee += landingPlan.Fee * landingPlanSummary.NumberOfPlans;
                }
            }

            bool isMonthPlan = currentPlan.TotalMonthPlan <= 1;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            int currentBaseMonth;

            if (isMonthPlan)
            {
                currentBaseMonth = currentMonthPlan - 1;
            }
            else
            {
                currentBaseMonth = currentMonthPlan > 0 ?
                    now.Day < 21 ? currentMonthPlan - 1 : currentMonthPlan :
                    0;
            }

            var differenceBetweenMonthPlans = currentPlan.TotalMonthPlan - currentBaseMonth;


            decimal totalFee = baseLandingPlansFee * differenceBetweenMonthPlans;
            decimal nextTotalFee = baseLandingPlansFee * currentPlan.TotalMonthPlan;

            if (discount is not null && discount.DiscountPlanFee > 0)
            {
                result.DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round((totalFee * discount.DiscountPlanFee) / 100, 2),
                    DiscountPercentage = discount.DiscountPlanFee,
                    MonthsToPay = differenceBetweenMonthPlans,
                    NextAmount = Math.Round((nextTotalFee * discount.DiscountPlanFee) / 100, 2),
                };

                totalFee -= result.DiscountPrepayment.Amount;
                nextTotalFee -= result.DiscountPrepayment.NextAmount;
            }

            result.Total = totalFee;
            result.CurrentMonthTotal = totalFee;
            result.NextMonthTotal = nextTotalFee;
            result.MajorThat21st = now.Day > 21;

            // Discount already paid plans
            if (lastLandingPlan is not null)
            {
                // Plan Downgrade
                if (result.CurrentMonthTotal <= lastLandingPlan.Fee)
                {
                    result.PositiveBalance = lastLandingPlan.Fee - result.CurrentMonthTotal;
                    result.Total = 0;
                    result.CurrentMonthTotal = 0;
                }
                else // Plan Upgrade
                {
                    result.Total -= lastLandingPlan.Fee;
                    result.CurrentMonthTotal -= lastLandingPlan.Fee;
                }
            }

            var nexMonnthInvoiceDate = now.AddMonths(differenceBetweenMonthPlans);
            result.NextMonthDate = new DateTime(nexMonnthInvoiceDate.Year, nexMonnthInvoiceDate.Month, 1);

            return result;
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
