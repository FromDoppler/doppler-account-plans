using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Model;
using System;

namespace Doppler.AccountPlans.Helpers
{
    public class MarketingPlan : ICalculateAmountDetalisHelper
    {
        public PlanAmountDetails CalculateAmountDetails(PlanInformation newPlan, ref PlanDiscountInformation newDiscount, ref UserPlan currentPlan, DateTime now, Promotion promotion, TimesApplyedPromocode timesAppliedPromocode, Promotion currentPromotion, DateTime? firstUpgradeDate, PlanDiscountInformation currentDiscountPlan, decimal creditsDiscount)
        {
            currentPlan ??= new UserPlan
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

            if (currentPlan.IdUserType == UserTypesEnum.Individual && newPlan.IdUserType != UserTypesEnum.Individual)
            {
                return ChangeIndividualToContacsOrEmails(newPlan, newDiscount, currentPlan, now, promotion, timesAppliedPromocode, currentPromotion, creditsDiscount);
            }
            else
            {
                var isMonthPlan = currentPlan.TotalMonthPlan <= 1;

                var currentMonthPlan = !isMonthPlan ?
                    currentPlan.CurrentMonthPlan :
                    1;

                var currentBaseMonth = currentMonthPlan > 0 &&
                    (currentPlan.IdUserType != UserTypesEnum.Individual && currentPlan.IdUserType != UserTypesEnum.Free) ?
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
                    DiscountPaymentAlreadyPaid = planAmount - discountAmountPromotion - discountAmountAdmin - currentDiscountPrepayment,
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

                result.Total = (newPlan.Fee * differenceBetweenMonthPlans) - result.DiscountPaymentAlreadyPaid - result.DiscountPrepayment.Amount;

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

                        int promocodeDuration = 0;
                        if (currentPromotion.Duration.HasValue)
                        {
                            promocodeDuration = currentPromotion.Duration.Value - timesAppliedPromocode.CountApplied;
                        }

                        result.DiscountPromocode = new DiscountPromocode
                        {
                            Amount = discount,
                            DiscountPercentage = currentPromotion.DiscountPercentage ?? 0,
                            ExtraCredits = currentPromotion.ExtraCredits ?? 0,
                            Duration = promocodeDuration
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
                    firstUpgradeDate != null && firstUpgradeDate.Value.Month == now.Month && firstUpgradeDate.Value.Year == now.Year && firstUpgradeDate.Value.Day >= 21 ?
                    result.Total : (differenceBetweenMonthPlans > 0 ? result.Total : 0) :
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

                result.NextMonthTotal = (newPlan.Fee * newDiscount.MonthPlan) - result.DiscountPlanFeeAdmin.NextAmount - nextDiscountPromocodeAmmount - result.DiscountPrepayment.NextAmount;
                result.MajorThat21st = now.Day > 21;

                var nexMonnthInvoiceDate = !isMonthPlan ? now.AddMonths(differenceBetweenMonthPlans) : now.AddMonths(1);
                result.NextMonthDate = new DateTime(nexMonnthInvoiceDate.Year, nexMonnthInvoiceDate.Month, 1);

                return result;
            }
        }

        private static int GetMonthsToDiscount(bool isMonthPlan, int differenceBetweenMonthPlans, UserTypesEnum idUserType)
        {
            if (idUserType == UserTypesEnum.Individual || idUserType == UserTypesEnum.Free)
                return 0;

            return (!isMonthPlan || differenceBetweenMonthPlans == 0) ? differenceBetweenMonthPlans : 1;
        }

        private static PlanAmountDetails ChangeIndividualToContacsOrEmails(
            PlanInformation newPlan,
            PlanDiscountInformation newDiscount,
            UserPlan currentPlan,
            DateTime now, Promotion promotion,
            TimesApplyedPromocode timesAppliedPromocode,
            Promotion currentPromotion,
            decimal creditsDiscount)
        {
            var differenceBetweenMonthPlans = newDiscount.MonthPlan;
            decimal amount = creditsDiscount;
            var currentDiscountPlanFeePromotion = currentPlan.DiscountPlanFeePromotion ?? 0;
            var currentDiscountPlanFeeAdmin = currentPlan.DiscountPlanFeeAdmin ?? 0;
            var planAmount = Math.Round(amount, 2);

            var discountAmountPromotion = Math.Round(((currentPlan.Fee * 1) * currentDiscountPlanFeePromotion) / 100, 2);
            var discountAmountAdmin = Math.Round((amount * currentDiscountPlanFeeAdmin) / 100, 2);

            var result = new PlanAmountDetails
            {
                DiscountPaymentAlreadyPaid = now.Day < 21 ? planAmount - discountAmountAdmin : 0,
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

            result.CurrentMonthTotal = now.Day > 21 ? 0 : (result.Total > 0 ? result.Total : 0);
            result.MajorThat21st = now.Day > 21;
            result.PositiveBalance = result.CurrentMonthTotal > 0 ? 0 : (now.Day < 21 ? (-1) * total : creditsDiscount);

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
    }
}
