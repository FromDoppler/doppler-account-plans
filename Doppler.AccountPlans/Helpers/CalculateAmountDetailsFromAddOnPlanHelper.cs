using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Doppler.AccountPlans.Helpers
{
    public class AddOnPlanHelper : ICalculateAmountDetalisHelper
    {
        public PlanAmountDetails CalculateAmountDetails(PlanTypeEnum newPlanType, PlanInformation newPlan, ref PlanDiscountInformation newDiscount, ref UserPlan currentPlan, DateTime now, Promotion promotion, TimesApplyedPromocode timesAppliedPromocode, Promotion currentPromotion, DateTime? firstUpgradeDate, PlanDiscountInformation currentDiscountPlan, decimal creditsDiscount)
        {
            currentPlan ??= new UserPlan
            {
                Fee = 0,
                CurrentMonthPlan = 0,
                IdUserType = UserTypesEnum.Free,
                AdditionalServices = []
            };

            var addOnType = GetAddonTypeByPlanType(newPlanType);
            var addOnPlan = currentPlan.AdditionalServices.FirstOrDefault(ads => ads.IdAddOnType == (int)addOnType);
            var addOnPlanFee = addOnPlan != null ? addOnPlan.Fee : 0;

            newDiscount ??= new PlanDiscountInformation
            {
                MonthPlan = currentDiscountPlan != null ? currentDiscountPlan.MonthPlan : 1,
                DiscountPlanFee = currentDiscountPlan != null ? currentDiscountPlan.DiscountPlanFee : 0,
                ApplyPromo = currentDiscountPlan == null || currentDiscountPlan.ApplyPromo
            };

            var isMonthPlan = currentPlan.TotalMonthPlan <= 1;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            var currentBaseMonth = currentMonthPlan > 0 && currentPlan.IdUserType != UserTypesEnum.Free ?
                isMonthPlan ?
                (now.Day < 21 ? currentMonthPlan - 1 :
                firstUpgradeDate != null && firstUpgradeDate.Value.Month == now.Month && firstUpgradeDate.Value.Year == now.Year && firstUpgradeDate.Value.Day >= 21 ?
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
                amount = (addOnPlanFee) * numberOfMonthsToDiscount;
                currentDiscountPrepayment = currentDiscountPlan != null ?
                    Math.Round((addOnPlanFee * numberOfMonthsToDiscount * currentDiscountPlan.DiscountPlanFee) / 100, 2) :
                    0;
            }
            else
            {
                numberOfMonthsToDiscount = GetMonthsToDiscount(isMonthPlan, currentBaseMonth, currentPlan.IdUserType);
                decimal ammountToDiscount = (addOnPlanFee * numberOfMonthsToDiscount);
                amount = addOnPlanFee * currentDiscountPlan.MonthPlan - ammountToDiscount;
                currentDiscountPrepayment = currentDiscountPlan != null ?
                    Math.Round(amount * currentDiscountPlan.DiscountPlanFee / 100, 2) :
                    0;
            }

            var currentDiscountPlanFeeAdmin = currentPlan.DiscountPlanFeeAdmin ?? 0;
            var planAmount = Math.Round(amount, 2);
            var discountAmountAdmin = Math.Round((amount * currentDiscountPlanFeeAdmin) / 100, 2);

            var result = new PlanAmountDetails
            {
                DiscountPaymentAlreadyPaid = planAmount - discountAmountAdmin - currentDiscountPrepayment,
                DiscountPrepayment = new DiscountPrepayment
                {
                    Amount = Math.Round(((newPlan.ChatPlanFee ?? 0) * differenceBetweenMonthPlans * newDiscount.DiscountPlanFee) / 100, 2),
                    DiscountPercentage = newDiscount.DiscountPlanFee,
                    MonthsToPay = differenceBetweenMonthPlans,
                    NextAmount = Math.Round(((newPlan.ChatPlanFee ?? 0) * newDiscount.MonthPlan * newDiscount.DiscountPlanFee) / 100, 2),
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

            result.Total = ((newPlan.ChatPlanFee ?? 0) * differenceBetweenMonthPlans) - result.DiscountPaymentAlreadyPaid - result.DiscountPrepayment.Amount;

            var count = 0;
            if (timesAppliedPromocode != null)
            {
                count = (now.Month == timesAppliedPromocode.LastMonthApplied && now.Year == timesAppliedPromocode.LastYearApplied) ? timesAppliedPromocode.CountApplied : timesAppliedPromocode.CountApplied + 1;
            }

            if (promotion != null && promotion.DiscountPercentage > 0)
            {
                var applyCurrentMonth = (now.Month == timesAppliedPromocode.LastMonthApplied && now.Year == timesAppliedPromocode.LastYearApplied);
                var duration = promotion.Duration.Value - (applyCurrentMonth ? timesAppliedPromocode.CountApplied - 1 : timesAppliedPromocode.CountApplied);

                if (duration >= 0)
                {
                    var discount = Math.Round(newPlan.ChatPlanFee.Value * promotion.DiscountPercentage.Value / 100, 2);

                    result.Total -= discount;
                    result.DiscountPromocode = new DiscountPromocode
                    {
                        Amount = discount,
                        DiscountPercentage = promotion.DiscountPercentage ?? 0,
                        Duration = duration
                    };
                }

                result.DiscountPrepayment.Amount = 0;
                result.DiscountPrepayment.DiscountPercentage = 0;
            }
            else
            {
                if (currentPromotion != null && (!currentPromotion.Duration.HasValue || currentPromotion.Duration.Value > 0))
                {
                    var discountPercentage = currentPromotion.DiscountPercentage ?? 0;
                    var discount = Math.Round(newPlan.ChatPlanFee.Value * discountPercentage / 100, 2);

                    result.Total -= discount;

                    int promocodeDuration = 0;
                    if (currentPromotion.Duration.HasValue)
                    {
                        promocodeDuration = currentPromotion.Duration.Value - 1;
                    }

                    result.DiscountPromocode = new DiscountPromocode
                    {
                        Amount = discount,
                        DiscountPercentage = currentPromotion.DiscountPercentage ?? 0,
                        Duration = promocodeDuration
                    };

                    result.DiscountPrepayment.Amount = 0;
                    result.DiscountPrepayment.DiscountPercentage = 0;
                }
            }

            if (currentPlan != null && currentPlan.DiscountPlanFeeAdmin.HasValue)
            {
                var discount = Math.Round((newPlan.ChatPlanFee ?? 0) * differenceBetweenMonthPlans * currentPlan.DiscountPlanFeeAdmin.Value / 100, 2);
                result.Total -= discount;
                result.DiscountPlanFeeAdmin = new DiscountPlanFeeAdmin
                {
                    Amount = discount,
                    DiscountPercentage = currentPlan.DiscountPlanFeeAdmin ?? 0,
                    NextAmount = Math.Round(((newPlan.ChatPlanFee ?? 0) * newDiscount.MonthPlan * currentPlan.DiscountPlanFeeAdmin.Value) / 100, 2),
                };
            }

            result.CurrentMonthTotal = (now.Day >= 21 && currentPlan.IdUserType != UserTypesEnum.Free) ?
                currentPlan.IdUserType != UserTypesEnum.Individual && result.DiscountPrepayment.MonthsToPay <= 1 ?
                firstUpgradeDate != null && firstUpgradeDate.Value.Month == now.Month && firstUpgradeDate.Value.Year == now.Year && firstUpgradeDate.Value.Day >= 21 ?
                result.Total : (differenceBetweenMonthPlans > 0 ? result.Total : 0) :
                result.Total :
                result.Total;

            //Check if for the next month apply the current promocode
            decimal nextDiscountPromocodeAmmount = 0;

            if (promotion != null && promotion.DiscountPercentage > 0 &&
                (!promotion.Duration.HasValue || (now.Day > 21 ? promotion.Duration.Value >= count : promotion.Duration.Value > count)))
            {
                nextDiscountPromocodeAmmount = Math.Round(newPlan.ChatPlanFee.Value * promotion.DiscountPercentage.Value / 100, 2);
            }
            else
            {
                if (currentPromotion != null)
                {
                    if (!currentPromotion.Duration.HasValue || currentPromotion.Duration.Value > count)
                    {
                        var discountPercentage = currentPromotion.DiscountPercentage ?? 0;
                        nextDiscountPromocodeAmmount = Math.Round(newPlan.ChatPlanFee.Value * discountPercentage / 100, 2);
                    }
                }
            }

            decimal nextMonthTotal = (newPlan.ChatPlanFee.Value * newDiscount.MonthPlan) - result.DiscountPlanFeeAdmin.NextAmount - nextDiscountPromocodeAmmount - result.DiscountPrepayment.NextAmount;

            result.NextMonthTotal = nextMonthTotal;
            //result.NextMonthTotal = ((newPlan.ChatPlanFee ?? 0) * newDiscount.MonthPlan) - result.DiscountPlanFeeAdmin.NextAmount - result.DiscountPrepayment.NextAmount;
            result.MajorThat21st = now.Day > 21;

            var nexMonnthInvoiceDate = !isMonthPlan ? now.AddMonths(differenceBetweenMonthPlans) : now.AddMonths(1);
            result.NextMonthDate = new DateTime(nexMonnthInvoiceDate.Year, nexMonnthInvoiceDate.Month, 1);

            return result;
        }

        private static int GetMonthsToDiscount(bool isMonthPlan, int differenceBetweenMonthPlans, UserTypesEnum idUserType)
        {
            if (idUserType == UserTypesEnum.Individual || idUserType == UserTypesEnum.Free)
                return 0;

            return (!isMonthPlan || differenceBetweenMonthPlans == 0) ? differenceBetweenMonthPlans : 1;
        }

        private static AddOnType? GetAddonTypeByPlanType(PlanTypeEnum planType)
        {
            return planType switch
            {
                PlanTypeEnum.Chat => (AddOnType?)AddOnType.Chat,
                PlanTypeEnum.OnSite => (AddOnType?)AddOnType.OnSite,
                PlanTypeEnum.PushNotification => (AddOnType?)AddOnType.PushNotification,
                PlanTypeEnum.Landing => (AddOnType?)AddOnType.Landing,
                _ => null,
            };
        }
    }
}
