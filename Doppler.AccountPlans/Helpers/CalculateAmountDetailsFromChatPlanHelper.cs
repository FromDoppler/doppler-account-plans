using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using System;

namespace Doppler.AccountPlans.Helpers
{
    public class ChatPlan : ICalculateAmountDetalisHelper
    {
        public PlanAmountDetails CalculateAmountDetails(PlanInformation newPlan, ref PlanDiscountInformation newDiscount, ref UserPlanInformation currentPlan, DateTime now, Promotion promotion, TimesApplyedPromocode timesAppliedPromocode, Promotion currentPromotion, UserPlanInformation firstUpgrade, PlanDiscountInformation currentDiscountPlan, decimal creditsDiscount)
        {
            currentPlan ??= new UserPlanInformation
            {
                Fee = 0,
                CurrentMonthPlan = 0,
                IdUserType = UserTypesEnum.Free
            };

            newDiscount ??= new PlanDiscountInformation
            {
                MonthPlan = currentDiscountPlan != null ? currentDiscountPlan.MonthPlan : 1,
                DiscountPlanFee = currentDiscountPlan != null ? currentDiscountPlan.DiscountPlanFee : 0,
                ApplyPromo = currentDiscountPlan != null ? currentDiscountPlan.ApplyPromo : true
            };

            var isMonthPlan = currentPlan.TotalMonthPlan <= 1;

            var currentMonthPlan = !isMonthPlan ?
                currentPlan.CurrentMonthPlan :
                1;

            var currentBaseMonth = currentMonthPlan > 0 && currentPlan.IdUserType != UserTypesEnum.Free ?
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
                amount = (currentPlan.ChatPlanFee ?? 0) * numberOfMonthsToDiscount;
                currentDiscountPrepayment = currentDiscountPlan != null ?
                    Math.Round(((currentPlan.ChatPlanFee ?? 0) * numberOfMonthsToDiscount * currentDiscountPlan.DiscountPlanFee) / 100, 2) :
                    0;
            }
            else
            {
                numberOfMonthsToDiscount = GetMonthsToDiscount(isMonthPlan, currentBaseMonth, currentPlan.IdUserType);
                decimal ammountToDiscount = ((currentPlan.ChatPlanFee ?? 0) * numberOfMonthsToDiscount);
                amount = (currentPlan.ChatPlanFee ?? 0) * currentDiscountPlan.MonthPlan - ammountToDiscount;
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
                firstUpgrade != null && firstUpgrade.Date.Month == now.Month && firstUpgrade.Date.Year == now.Year && firstUpgrade.Date.Day >= 21 ?
                result.Total : (differenceBetweenMonthPlans > 0 ? result.Total : 0) :
                result.Total :
                result.Total;

            result.NextMonthTotal = ((newPlan.ChatPlanFee ?? 0) * newDiscount.MonthPlan) - result.DiscountPlanFeeAdmin.NextAmount - result.DiscountPrepayment.NextAmount;
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
    }
}
