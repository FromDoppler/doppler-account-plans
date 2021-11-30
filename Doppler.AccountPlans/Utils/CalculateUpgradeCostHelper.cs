using Doppler.AccountPlans.Model;
using System;
using Doppler.AccountPlans.Enums;

namespace Doppler.AccountPlans.Utils
{
    public static class CalculateUpgradeCostHelper
    {
        public static PlanAmountDetails CalculatePlanAmountDetails(PlanInformation newPlan, PlanDiscountInformation newDiscount, PlanInformation currentPlan, DateTime now, Promotion promotion)
        {
            currentPlan ??= new PlanInformation
            {
                Fee = 0,
                CurrentMonthPlan = 0,
                IdUserType = UserTypesEnum.Free
            };

            newDiscount ??= new PlanDiscountInformation
            {
                MonthPlan = 1,
                DiscountPlanFee = 0,
                ApplyPromo = true
            };

            var isMonthPlan = currentPlan.CurrentMonthPlan <= 1;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            var currentBaseMonth = now.Day < 21 ?
                currentMonthPlan - 1 :
                currentMonthPlan;

            var differenceBetweenMonthPlans = newDiscount.MonthPlan - currentBaseMonth;

            var numberOfMonthsToDiscount = GetMonthsToDiscount(isMonthPlan, differenceBetweenMonthPlans, currentPlan.IdUserType);

            var result = new PlanAmountDetails
            {
                DiscountPaymentAlreadyPaid = Math.Round(currentPlan.Fee * numberOfMonthsToDiscount, 2),
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round((newPlan.Fee * newDiscount.MonthPlan * newDiscount.DiscountPlanFee) / 100, 2),
                    DiscountPercentage = newDiscount.DiscountPlanFee
                },
                DiscountPromocode = new DiscountPromocode
                {
                    Amount = 0,
                    DiscountPercentage = 0
                }
            };

            result.Total = (newPlan.Fee * newDiscount.MonthPlan) - result.DiscountPaymentAlreadyPaid - result.DiscountPrepayment.Amount;

            if (promotion != null && newDiscount.ApplyPromo && promotion.DiscountPercentage > 0)
            {
                var discount = Math.Round(newPlan.Fee * promotion.DiscountPercentage.Value / 100, 2);

                result.Total -= discount;
                result.DiscountPromocode = new DiscountPromocode
                {
                    Amount = discount,
                    DiscountPercentage = promotion.DiscountPercentage ?? 0
                };

                result.DiscountPrepayment.Amount = 0;
                result.DiscountPrepayment.DiscountPercentage = 0;
            }

            return result;
        }

        private static int GetMonthsToDiscount(bool isMonthPlan, int differenceBetweenMonthPlans, UserTypesEnum idUserType)
        {
            if (idUserType == UserTypesEnum.Individual || idUserType == UserTypesEnum.Free)
                return 0;

            return (!isMonthPlan || differenceBetweenMonthPlans == 0) ? differenceBetweenMonthPlans : 1;
        }
    }
}
