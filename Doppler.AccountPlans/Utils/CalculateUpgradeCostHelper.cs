using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Doppler.AccountPlans.Utils
{
    public static class CalculateUpgradeCostHelper
    {
        public static PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan, IList<TaxSettings> taxesSettings, DateTime now)
        {
            if (currentPlan == null) //update from free
            {
                currentPlan = new PlanInformation
                {
                    Fee = 0,
                    CurrentMonthPlan = 0
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

            var total = (newPlan.Fee * newDiscount.MonthPlan) - result.DiscountPaymentAlreadyPaid - result.DiscountPrepayment.Amount;
            result.Taxes = taxesSettings != null ? taxesSettings.Select(t => new Tax { Percentage = t.Percentage, Amount = (total * t.Percentage) / 100 }).ToList() : new List<Tax>();
            result.Total = total + result.Taxes.Sum(t => t.Amount);

            return result;
        }
    }
}
