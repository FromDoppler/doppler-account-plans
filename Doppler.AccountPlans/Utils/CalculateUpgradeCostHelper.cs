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
            UserPlan currentPlan,
            DateTime now, Promotion promotion,
            TimesApplyedPromocode timesAppliedPromocode,
            Promotion currentPromotion,
            DateTime? firstUpgradeDate,
            PlanDiscountInformation currentDiscountPlan,
            decimal creditsDiscount,
            PlanTypeEnum planType)
        {
            return GetHelper(planType).CalculateAmountDetails(planType, newPlan, ref newDiscount, ref currentPlan, now, promotion, timesAppliedPromocode, currentPromotion, firstUpgradeDate, currentDiscountPlan, creditsDiscount);
        }

        public static PlanAmountDetails CalculateLandingPlanAmountDetails(
            UserPlan currentPlan,
            DateTime now,
            List<LandingPlanSummary> landingPlansSummary,
            IEnumerable<LandingPlanInformation> landingsPlanInformation,
            PlanDiscountInformation discount,
            UserPlanInformation lastLandingPlan,
            DateTime? firstUpgradeDate,
            Promotion promotion,
            Promotion currentPromotion,
            TimesApplyedPromocode timesAppliedPromocode)
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

            var currentBaseMonth = currentMonthPlan > 0 && currentPlan.IdUserType != UserTypesEnum.Free ?
                isMonthPlan ?
                (now.Day < 21 ? currentMonthPlan - 1 :
                firstUpgradeDate != null && firstUpgradeDate.Value.Date.Month == now.Month && firstUpgradeDate.Value.Date.Year == now.Year && firstUpgradeDate.Value.Date.Day >= 21 ?
                currentMonthPlan - 1 : currentMonthPlan) :
                now.Day < 21 ? currentMonthPlan - 1 : currentMonthPlan :
                0;

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

            result.Total = totalFee;

            //Discount already paid plans
            if (lastLandingPlan is not null)
            {
                decimal amount = 0;
                var numberOfMonthsToDiscount = 0;

                var baseMonth = isMonthPlan ?
                                now.Day < 21 ? 1 :
                                (firstUpgradeDate.Value.Date.Month == now.Month &&
                                firstUpgradeDate.Value.Date.Year == now.Year &&
                                firstUpgradeDate.Value.Date.Day >= 21) ? 1 : 0 :
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

            if (promotion != null && promotion.DiscountPercentage > 0)
            {
                var discountPromotion = Math.Round(totalFee * promotion.DiscountPercentage.Value / 100, 2);

                if (promotion.IdAddOnPlan.HasValue)
                {
                    var landingWithDiscount = landingPlansSummary.FirstOrDefault(l => l.IdLandingPlan == promotion.IdAddOnPlan);

                    if (landingWithDiscount != null)
                    {
                        var landingPlan = landingsPlanInformation.FirstOrDefault(x => x.PlanId == landingWithDiscount.IdLandingPlan);
                        discountPromotion = Math.Round((landingPlan.Fee * landingWithDiscount.NumberOfPlans) * promotion.DiscountPercentage.Value / 100, 2);
                    }
                    else
                    {
                        discountPromotion = 0;
                    }
                }

                result.Total -= discountPromotion;
                result.DiscountPromocode = new DiscountPromocode
                {
                    Amount = discountPromotion,
                    DiscountPercentage = discountPromotion > 0 ? promotion.DiscountPercentage ?? 0 : 0,
                    Duration = discountPromotion > 0 ? promotion.Duration ?? 0 : 0
                };

                result.DiscountPrepayment.Amount = 0;
                result.DiscountPrepayment.DiscountPercentage = 0;
            }
            else
            {
                if (currentPromotion != null && (!currentPromotion.Duration.HasValue || currentPromotion.Duration.Value > 0))
                {
                    var discountPercentage = currentPromotion.DiscountPercentage ?? 0;
                    var discountPromotion = Math.Round(totalFee * discountPercentage / 100, 2);

                    if (currentPromotion.IdAddOnPlan.HasValue)
                    {
                        var landingWithDiscount = landingPlansSummary.FirstOrDefault(l => l.IdLandingPlan == currentPromotion.IdAddOnPlan);

                        if (landingWithDiscount != null)
                        {
                            var landingPlan = landingsPlanInformation.FirstOrDefault(x => x.PlanId == landingWithDiscount.IdLandingPlan);
                            discountPromotion = Math.Round((landingPlan.Fee * landingWithDiscount.NumberOfPlans) * currentPromotion.DiscountPercentage.Value / 100, 2);
                        }
                        else
                        {
                            discountPromotion = 0;
                        }
                    }

                    result.Total -= discountPromotion;

                    int promocodeDuration = 0;
                    if (currentPromotion.Duration.HasValue)
                    {
                        promocodeDuration = currentPromotion.Duration.Value - 1;
                    }

                    result.DiscountPromocode = new DiscountPromocode
                    {
                        Amount = discountPromotion,
                        DiscountPercentage = discountPromotion > 0 ? currentPromotion.DiscountPercentage ?? 0 : 0,
                        Duration = discountPromotion > 0 ? promocodeDuration : 0
                    };

                    result.DiscountPrepayment.Amount = 0;
                    result.DiscountPrepayment.DiscountPercentage = 0;
                }
            }

            result.Total = result.Total - result.DiscountPrepayment.Amount - result.DiscountPaymentAlreadyPaid;
            result.CurrentMonthTotal = result.Total > 0 ? result.Total : 0;

            //Check if for the next month apply the current promocode
            decimal nextDiscountPromocodeAmmount = 0;

            if (promotion != null && promotion.DiscountPercentage > 0 &&
                    (!promotion.Duration.HasValue || promotion.Duration.Value > 1))
            {
                nextDiscountPromocodeAmmount = Math.Round(totalFee * promotion.DiscountPercentage.Value / 100, 2);

                if (promotion.IdAddOnPlan.HasValue)
                {
                    var landingWithDiscount = landingPlansSummary.FirstOrDefault(l => l.IdLandingPlan == promotion.IdAddOnPlan);

                    if (landingWithDiscount != null)
                    {
                        var landingPlan = landingsPlanInformation.FirstOrDefault(x => x.PlanId == landingWithDiscount.IdLandingPlan);
                        nextDiscountPromocodeAmmount = Math.Round((landingPlan.Fee * landingWithDiscount.NumberOfPlans) * promotion.DiscountPercentage.Value / 100, 2);
                    }
                    else
                    {
                        nextDiscountPromocodeAmmount = 0;
                    }
                }
            }
            else
            {
                if (currentPromotion != null)
                {
                    var count = (now.Month == timesAppliedPromocode.LastMonthApplied && now.Year == timesAppliedPromocode.LastYearApplied) ? timesAppliedPromocode.CountApplied : timesAppliedPromocode.CountApplied + 1;
                    if (!currentPromotion.Duration.HasValue || currentPromotion.Duration.Value > count)
                    {
                        var discountPercentage = currentPromotion.DiscountPercentage ?? 0;
                        nextDiscountPromocodeAmmount = Math.Round(totalFee * discountPercentage / 100, 2);

                        if (currentPromotion.IdAddOnPlan.HasValue)
                        {
                            var landingWithDiscount = landingPlansSummary.FirstOrDefault(l => l.IdLandingPlan == currentPromotion.IdAddOnPlan);

                            if (landingWithDiscount != null)
                            {
                                var landingPlan = landingsPlanInformation.FirstOrDefault(x => x.PlanId == landingWithDiscount.IdLandingPlan);
                                nextDiscountPromocodeAmmount = Math.Round((landingPlan.Fee * landingWithDiscount.NumberOfPlans) * currentPromotion.DiscountPercentage.Value / 100, 2);
                            }
                            else
                            {
                                nextDiscountPromocodeAmmount = 0;
                            }
                        }
                    }
                }
            }

            //nextTotalFee -= result.DiscountPrepayment.NextAmount;
            nextTotalFee = nextTotalFee - nextDiscountPromocodeAmmount - result.DiscountPrepayment.NextAmount;

            result.NextMonthTotal = nextTotalFee;
            result.MajorThat21st = now.Day > 21;
            result.PositiveBalance = result.CurrentMonthTotal > 0 ? 0 : result.Total;

            var nexMonnthInvoiceDate = !isMonthPlan ? now.AddMonths(differenceBetweenMonthPlans) : now.AddMonths(1);
            result.NextMonthDate = new DateTime(nexMonnthInvoiceDate.Year, nexMonnthInvoiceDate.Month, 1);

            return result;
        }

        private static ICalculateAmountDetalisHelper GetHelper(PlanTypeEnum planType)
        {
            return planType switch
            {
                PlanTypeEnum.Marketing => new MarketingPlan(),
                PlanTypeEnum.Chat or PlanTypeEnum.OnSite or PlanTypeEnum.PushNotification => new AddOnPlanHelper(),
                _ => null,
            };
        }
    }
}
