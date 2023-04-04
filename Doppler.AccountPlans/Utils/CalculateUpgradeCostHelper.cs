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
            UserPlanInformation firstUpgrade,
            PlanDiscountInformation currentDiscountPlan,
            decimal creditsDiscount)
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

            var isMonthPlan = currentPlan.TotalMonthPlan <= 1;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            var currentBaseMonth = currentMonthPlan > 0 &&
                (currentPlan.IdUserType != UserTypesEnum.Individual && currentPlan.IdUserType != UserTypesEnum.Free) ?
                isMonthPlan ?
                (now.Day < 21 ? currentMonthPlan - 1 :
                firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                currentMonthPlan - 1 : currentMonthPlan) :
                now.Day < 21 ? currentMonthPlan - 1 : currentMonthPlan :
                0;

            var differenceBetweenMonthPlans = newDiscount.MonthPlan - currentBaseMonth;

            int numberOfMonthsToDiscount;
            decimal currentDiscountPrepayment;
            decimal amount;

            if (isMonthPlan)
            {
                numberOfMonthsToDiscount = GetMonthsToDiscount(isMonthPlan, differenceBetweenMonthPlans, currentPlan.IdUserType);
                amount = currentPlan.Fee * numberOfMonthsToDiscount;
                currentDiscountPrepayment = currentDiscountPlan != null ?
                    Math.Round((currentPlan.Fee * numberOfMonthsToDiscount * currentDiscountPlan.DiscountPlanFee) / 100, 2) :
                    0;
            }
            else
            {
                numberOfMonthsToDiscount = GetMonthsToDiscount(isMonthPlan, currentBaseMonth, currentPlan.IdUserType);
                decimal ammountToDiscount = (currentPlan.Fee * numberOfMonthsToDiscount);
                amount = currentPlan.Fee * currentDiscountPlan.MonthPlan - ammountToDiscount;
                currentDiscountPrepayment = currentDiscountPlan != null ?
                    Math.Round(amount * currentDiscountPlan.DiscountPlanFee / 100, 2) :
                    0;
            }


            var currentDiscountPlanFeePromotion = currentPlan.DiscountPlanFeePromotion ?? 0;
            var currentDiscountPlanFeeAdmin = currentPlan.DiscountPlanFeeAdmin ?? 0;

            var planAmount = Math.Round(amount, 2);
            var discountAmountPromotion = Math.Round(((currentPlan.Fee * numberOfMonthsToDiscount) * currentDiscountPlanFeePromotion) / 100, 2);
            var discountAmountAdmin = Math.Round((amount * currentDiscountPlanFeeAdmin) / 100, 2);

            var result = new PlanAmountDetails
            {
                DiscountPaymentAlreadyPaid = (planAmount - discountAmountPromotion - discountAmountAdmin - currentDiscountPrepayment) + creditsDiscount,
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round((newPlan.Fee * differenceBetweenMonthPlans * newDiscount.DiscountPlanFee) / 100, 2),
                    DiscountPercentage = newDiscount.DiscountPlanFee,
                    MonthsToPay = differenceBetweenMonthPlans,
                    NextAmount = Math.Round((newPlan.Fee * newDiscount.MonthPlan * newDiscount.DiscountPlanFee) / 100, 2),
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

            var total = (newPlan.Fee * differenceBetweenMonthPlans) - result.DiscountPaymentAlreadyPaid - result.DiscountPrepayment.Amount;
            result.Total = total > 0 ? total : 0;
            result.PositiveBalance = result.Total > 0 ? 0 : (-1) * total;

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
                    var discountPercentage = currentPromotion.DiscountPercentage.HasValue ? currentPromotion.DiscountPercentage.Value : 0;
                    var discount = Math.Round(newPlan.Fee * discountPercentage / 100, 2);

                    result.Total -= discount;
                    result.DiscountPromocode = new DiscountPromocode
                    {
                        Amount = discount,
                        DiscountPercentage = currentPromotion.DiscountPercentage ?? 0,
                        ExtraCredits = currentPromotion.ExtraCredits ?? 0
                    };

                    result.DiscountPrepayment.Amount = 0;
                    result.DiscountPrepayment.DiscountPercentage = 0;
                }
            }

            if (currentPlan != null && currentPlan.DiscountPlanFeeAdmin.HasValue)
            {
                var discount = Math.Round(newPlan.Fee * differenceBetweenMonthPlans * currentPlan.DiscountPlanFeeAdmin.Value / 100, 2);
                result.Total -= discount;
                result.DiscountPlanFeeAdmin = new DiscountPlanFeeAdmin
                {
                    Amount = discount,
                    DiscountPercentage = currentPlan.DiscountPlanFeeAdmin ?? 0,
                    NextAmount = Math.Round((newPlan.Fee * newDiscount.MonthPlan * currentPlan.DiscountPlanFeeAdmin.Value) / 100, 2),
                };
            }

            result.CurrentMonthTotal = (now.Day >= 21 && currentPlan.IdUserType != UserTypesEnum.Free) ?
                currentPlan.IdUserType != UserTypesEnum.Individual && result.DiscountPrepayment.MonthsToPay <= 1 ?
                firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                result.Total : 0 :
                result.Total :
                result.Total;

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
                        var discountPercentage = currentPromotion.DiscountPercentage.HasValue ? currentPromotion.DiscountPercentage.Value : 0;
                        nextDiscountPromocodeAmmount = Math.Round(newPlan.Fee * discountPercentage / 100, 2);
                    }
                }
            }

            var nextMonthTotal = (newPlan.Fee * newDiscount.MonthPlan) - result.DiscountPlanFeeAdmin.NextAmount - nextDiscountPromocodeAmmount - result.DiscountPrepayment.NextAmount - result.PositiveBalance;
            result.NextMonthTotal = nextMonthTotal > 0 ? nextMonthTotal : 0;

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
