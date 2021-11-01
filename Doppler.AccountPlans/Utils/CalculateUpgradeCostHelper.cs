using Doppler.AccountPlans.Model;
using System;

namespace Doppler.AccountPlans.Utils
{
    public static class CalculateUpgradeCostHelper
    {
        public static PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan, DateTime now)
        {
            if (currentPlan == null) //update from free
            {
                currentPlan = new PlanInformation
                {
                    Fee = 0,
                    CurrentMonthPlan = 0
                };
            }

            if (newDiscount == null)
            {
                newDiscount = new PlanDiscountInformation
                {
                    MonthPlan = 1,
                    DiscountPlanFee = 0
                };
            }

            var isMonthPlan = currentPlan.CurrentMonthPlan == 0;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            var currentBaseMonth = now.Day < 21 ?
                currentMonthPlan - 1 :
                currentMonthPlan;

            var differenceBetweenMonthPlans = newDiscount.MonthPlan - currentBaseMonth;

            var numberOfMonthsToDiscount = (!isMonthPlan || differenceBetweenMonthPlans == 0) ?
                differenceBetweenMonthPlans :
                1;

            var result = new PlanAmountDetails
            {
                DiscountPaymentAlreadyPaid = Math.Round(currentPlan.Fee * numberOfMonthsToDiscount, MidpointRounding.AwayFromZero),
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round((newPlan.Fee * newDiscount.MonthPlan * newDiscount.DiscountPlanFee) / 100, MidpointRounding.AwayFromZero),
                    DiscountPercentage = newDiscount.DiscountPlanFee
                }
            };

            result.Total = (newPlan.Fee * newDiscount.MonthPlan) - result.DiscountPaymentAlreadyPaid - result.DiscountPrepayment.Amount;

            return result;
        }
    }
}
