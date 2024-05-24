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
            UserPlanInformation lastLandingPlan,
            UserPlanInformation firstUpgrade)
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

            discount ??= new PlanDiscountInformation
            {
                MonthPlan = 1,
                DiscountPlanFee = 0,
                ApplyPromo = true
            };

            bool isMonthPlan = currentPlan.TotalMonthPlan <= 1;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            int currentBaseMonth;

            if (isMonthPlan)
            {
                currentBaseMonth = currentMonthPlan > 0 && firstUpgrade != null ?
                    firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                    currentMonthPlan - 1 : 0 :
                    0;

                //currentBaseMonth = now.Day < 21 ? currentMonthPlan - 1 :
                //    firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                //    currentMonthPlan - 1 : firstUpgrade != null ? currentMonthPlan : currentMonthPlan - 1;
            }
            else
            {
                currentBaseMonth = currentMonthPlan > 0 && firstUpgrade != null ?
                    now.Day < 21 ? currentMonthPlan - 1 : currentMonthPlan :
                    0;
            }

            var differenceBetweenMonthPlans = discount.MonthPlan - currentBaseMonth;


            decimal totalFee = baseLandingPlansFee * differenceBetweenMonthPlans;
            decimal nextTotalFee = baseLandingPlansFee * discount.MonthPlan;

            result.DiscountPrepayment = new DiscountPrepayment
            {
                Amount = Math.Round((totalFee * discount.DiscountPlanFee) / 100, 2),
                DiscountPercentage = discount.DiscountPlanFee,
                MonthsToPay = differenceBetweenMonthPlans,
                NextAmount = Math.Round((nextTotalFee * discount.DiscountPlanFee) / 100, 2),
            };

            //Discount already paid plans
            if (lastLandingPlan is not null)
            {
                decimal amount = 0;
                var numberOfMonthsToDiscount = 0;

                var baseMonth = isMonthPlan ?
                                now.Day < 21 ? 1 :
                                (firstUpgrade.Date.Month == now.Month &&
                                firstUpgrade.Date.Year == now.Year &&
                                firstUpgrade.Date.Day >= 21) ? 1 : 0 :
                                now.Day < 21 ? currentMonthPlan - 1 : currentMonthPlan;

                /* Calculate payment already paid */
                if (isMonthPlan)
                {
                    numberOfMonthsToDiscount = baseMonth;
                    amount = lastLandingPlan.Fee * numberOfMonthsToDiscount;
                }
                else
                {
                    var fee = (lastLandingPlan.Fee * currentPlan.TotalMonthPlan) - Math.Round((lastLandingPlan.Fee * currentPlan.TotalMonthPlan * discount.DiscountPlanFee) / 100, 2);
                    numberOfMonthsToDiscount = currentPlan.TotalMonthPlan - baseMonth;
                    amount = numberOfMonthsToDiscount > 0 ? (decimal)((fee / currentPlan.TotalMonthPlan)) * numberOfMonthsToDiscount : 0.0m;
                }

                result.DiscountPaymentAlreadyPaid = Math.Round(amount, 2);
            }

            totalFee = totalFee - result.DiscountPrepayment.Amount - result.DiscountPaymentAlreadyPaid;
            nextTotalFee -= result.DiscountPrepayment.NextAmount;

            result.Total = totalFee;
            result.CurrentMonthTotal = result.Total > 0 ? result.Total : 0;
            result.NextMonthTotal = nextTotalFee;
            result.MajorThat21st = now.Day > 21;
            result.PositiveBalance = result.CurrentMonthTotal > 0 ? 0 : result.Total;

            var nexMonnthInvoiceDate = now.AddMonths(!isMonthPlan ? differenceBetweenMonthPlans : 1);
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
