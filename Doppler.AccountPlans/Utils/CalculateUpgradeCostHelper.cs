using Doppler.AccountPlans.Model;
using System;
using Doppler.AccountPlans.Enums;

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
            UserPlanInformation firstUpgrade)
        {
            currentPlan ??= new UserPlanInformation
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
                firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                currentMonthPlan - 1 : currentMonthPlan;

            var differenceBetweenMonthPlans = newDiscount.MonthPlan - currentBaseMonth;

            var numberOfMonthsToDiscount = GetMonthsToDiscount(isMonthPlan, differenceBetweenMonthPlans, currentPlan.IdUserType);

            var currentDiscountPlanFeePromotion = currentPlan.DiscountPlanFeePromotion ?? 0;
            var currentDiscountPlanFeeAdmin = currentPlan.DiscountPlanFeeAdmin ?? 0;

            var planAmount = Math.Round(currentPlan.Fee * numberOfMonthsToDiscount, 2);
            var discountAmountPromotion = Math.Round(((currentPlan.Fee * numberOfMonthsToDiscount) * currentDiscountPlanFeePromotion) / 100, 2);
            var discountAmountAdmin = Math.Round(((currentPlan.Fee * numberOfMonthsToDiscount) * currentDiscountPlanFeeAdmin) / 100, 2);

            var result = new PlanAmountDetails
            {
                DiscountPaymentAlreadyPaid = planAmount - discountAmountPromotion - discountAmountAdmin,
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round((newPlan.Fee * newDiscount.MonthPlan * newDiscount.DiscountPlanFee) / 100, 2),
                    DiscountPercentage = newDiscount.DiscountPlanFee
                },
                DiscountPromocode = new DiscountPromocode
                {
                    Amount = 0,
                    DiscountPercentage = 0
                },
                DiscountPlanFeeAdmin = new DiscountPlanFeeAdmin
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
            else
            {
                if (currentPromotion != null && (!currentPromotion.Duration.HasValue || currentPromotion.Duration.Value > timesAppliedPromocode.CountApplied))
                {
                    var discount = Math.Round(newPlan.Fee * currentPromotion.DiscountPercentage.Value / 100, 2);

                    result.Total -= discount;
                    result.DiscountPromocode = new DiscountPromocode
                    {
                        Amount = discount,
                        DiscountPercentage = currentPromotion.DiscountPercentage ?? 0
                    };

                    result.DiscountPrepayment.Amount = 0;
                    result.DiscountPrepayment.DiscountPercentage = 0;
                }
            }

            if (currentPlan != null && currentPlan.DiscountPlanFeeAdmin.HasValue)
            {
                var discount = Math.Round(newPlan.Fee * currentPlan.DiscountPlanFeeAdmin.Value / 100, 2);
                result.Total -= discount;
                result.DiscountPlanFeeAdmin = new DiscountPlanFeeAdmin
                {
                    Amount = discount,
                    DiscountPercentage = currentPlan.DiscountPlanFeeAdmin ?? 0
                };
            }

            result.CurrentMonthTotal = now.Day >= 21 && currentPlan.IdUserType != UserTypesEnum.Free ?
                firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                result.Total : 0 : result.Total;

            //Check if for the next month apply the current promocode
            decimal nextDiscountPromocodeAmmount = 0;

            if (promotion != null && newDiscount.ApplyPromo && promotion.DiscountPercentage > 0 &&
                (!promotion.Duration.HasValue || promotion.Duration.Value > 1))
            {
                nextDiscountPromocodeAmmount = Math.Round(newPlan.Fee * promotion.DiscountPercentage.Value / 100, 2);
            }
            else
            {
                if (currentPromotion != null)
                {
                    var count = (now.Month == timesAppliedPromocode.LastMonthApplied && now.Year == timesAppliedPromocode.LastYearApplied) ? timesAppliedPromocode.CountApplied : timesAppliedPromocode.CountApplied + 1;
                    if (!currentPromotion.Duration.HasValue || currentPromotion.Duration.Value > count)
                    {
                        nextDiscountPromocodeAmmount = Math.Round(newPlan.Fee * currentPromotion.DiscountPercentage.Value / 100, 2);
                    }
                }
            }

            result.NextMonthTotal = (newPlan.Fee * newDiscount.MonthPlan) - result.DiscountPlanFeeAdmin.Amount - nextDiscountPromocodeAmmount;

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
