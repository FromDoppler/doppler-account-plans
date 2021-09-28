using Doppler.AccountPlans.Model;
using System;

namespace Doppler.AccountPlans.Utils
{
    public static class CalculateUpgradeCostHelper
    {
        public static PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan, DateTime now)
        {
            var currentBaseMonth = now.Day >= 21 ? currentPlan.CurrentMonthPlan : currentPlan.CurrentMonthPlan - 1;
            var totalPlan = newDiscount.MonthPlan != currentBaseMonth ? (newPlan.Fee - currentPlan.Fee) * (1 - (newDiscount.DiscountPlanFee / 100)) : newPlan.Fee * (1 - (newDiscount.DiscountPlanFee / 100));

            return new PlanAmountDetails
            {
                Total = Math.Round((newDiscount.MonthPlan != currentBaseMonth ? totalPlan * (newDiscount.MonthPlan - currentBaseMonth) : totalPlan * newDiscount.MonthPlan), MidpointRounding.AwayFromZero),
                DiscountPaymentAlreadyPaid = Math.Round((currentPlan.Fee * (newDiscount.MonthPlan - currentBaseMonth)), MidpointRounding.AwayFromZero),
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round(((newPlan.Fee * newDiscount.DiscountPlanFee) / 100), MidpointRounding.AwayFromZero),
                    DiscountPercentage = newDiscount.DiscountPlanFee
                }
            };
        }
    }
}
