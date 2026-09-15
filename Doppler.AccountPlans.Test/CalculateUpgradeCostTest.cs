using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;
using Doppler.AccountPlans.Controllers;
using Doppler.AccountPlans.Encryption;
using Doppler.AccountPlans.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Doppler.AccountPlans.Enums;
using System.Threading.Tasks;
using Xunit;
using System.Collections.Generic;

namespace Doppler.AccountPlans
{
    public class CalculateUpgradeCostTest
    {

        [Fact]
        public void CalculateUpgradeCostHelper_Free_to_Monthly_method_should_return_correct_value_when_is_free_client()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 0,
                MonthPlan = 1
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(5, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Free_to_Quarterly_method_should_return_correct_value_when_is_free_client()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 5,
                MonthPlan = 3
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(14.25m, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Free_to_Biannually_method_should_return_correct_value_when_is_free_client()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 15,
                MonthPlan = 6
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(25.5m, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Free_to_Annually_method_should_return_correct_value_when_is_free_client()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 25,
                MonthPlan = 12
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(45, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Monthly_method_should_return_correct_value_when_day_is_minor_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 0,
                MonthPlan = 1
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(3, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Monthly_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 0,
                MonthPlan = 1
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(0, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Monthly_method_should_return_correct_value_when_day_is_major_that_21_and_plan_month_is_detected_correctly()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 0,
                MonthPlan = 1
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(0, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Monthly_to_Quarterly_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 5,
                MonthPlan = 3
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
                IdUserType = UserTypesEnum.Monthly
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 0,
                IdUserType = UserTypesEnum.Monthly
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(7.5m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Monthly_to_Biannually_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 15,
                MonthPlan = 6
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(19.25m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Monthly_to_Annually_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 25,
                MonthPlan = 12
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(39.25m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Quarterly_method_should_return_correct_value_when_day_is_minor_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 5,
                MonthPlan = 3
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(12.25m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Quarterly_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 21));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 5,
                MonthPlan = 3
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(7.5m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Biannually_method_should_return_correct_value_when_day_is_minor_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 15,
                MonthPlan = 6
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 2
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);
            // Assert
            Assert.Equal(23.5m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Biannually_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 15,
                MonthPlan = 6
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 2
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(19.25m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Annually_method_should_return_correct_value_when_day_is_minor_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 6));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 25,
                MonthPlan = 12
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 4
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(43, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Annually_method_should_return_correct_value_when_day_is_major_that_21()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var currentDiscountPlan = new PlanDiscountInformation
            {
                DiscountPlanFee = 25,
                MonthPlan = 12
            };

            var newPlan = new PlanInformation
            {
                Fee = 5,
                IdUserType = UserTypesEnum.Subscribers
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 4,
                IdUserType = UserTypesEnum.Subscribers
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(39.25m, result.Total);
            Assert.Equal(2, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Prepaid_method_should_return_correct_value_when_newPlan_is_prepaid()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var newPlan = new PlanInformation
            {
                Fee = 5,
                IdUserType = UserTypesEnum.Individual
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                CurrentMonthPlan = 4,
                IdUserType = UserTypesEnum.Individual
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, null, currentPlan, dateTimeProviderMock.Object.Now, new Promotion(), null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(5, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_Prepaid_method_should_return_correct_value_when_newPlan_is_monthly()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 22));

            var newPlan = new PlanInformation
            {
                Fee = 15,
                IdUserType = UserTypesEnum.Monthly
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                IdUserType = UserTypesEnum.Individual
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, null, currentPlan, dateTimeProviderMock.Object.Now, new Promotion(), null, null, null, null, 2, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(15, result.Total);
            Assert.Equal(13, result.NextMonthTotal);
            Assert.Equal(2, result.PositiveBalance);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
        }

        [Theory]
        [InlineData(14.55, 3, 0.45, 15)]
        [InlineData(13.5, 10, 1.5, 15)]
        [InlineData(500, 50, 500, 1000)]
        [InlineData(990, 1, 10, 1000)]
        [InlineData(1000, 0, 0, 1000)]
        public void CalculateUpgradeCostHelper_Prepaid_promocode_should_return_correct_value_when_newPlan_has_discount_percentage(
            decimal total,
            int discountPercentage,
            decimal discountPromocode,
            int planFee)
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 20));

            var newPlan = new PlanInformation
            {
                Fee = planFee,
                IdUserType = UserTypesEnum.Monthly
            };

            var currentPlan = new UserPlan
            {
                Fee = 2,
                IdUserType = UserTypesEnum.Individual,
                TotalMonthPlan = 1
            };

            var promotion = new Promotion
            {
                DiscountPercentage = discountPercentage
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, null, currentPlan, dateTimeProviderMock.Object.Now, promotion, null, null, null, null, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(total, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
            Assert.Equal(0, result.DiscountPrepayment.Amount);
            Assert.Equal(0, result.DiscountPrepayment.DiscountPercentage);
            Assert.Equal(discountPercentage, result.DiscountPromocode.DiscountPercentage);
            Assert.Equal(discountPromocode, result.DiscountPromocode.Amount);
        }

        [Theory]
        [InlineData(9, 8, 15, 5, 3, 0, 19.95, 22.8, 2.25)]
        [InlineData(9, 8, 15, 5, 3, 1, 19.95, 22.8, 2.25)]
        [InlineData(9, 8, 15, 5, 3, 2, 13.30, 15.2, 1.5)]
        [InlineData(9, 8, 15, 5, 3, 3, 6.65, 7.60, 0.75)]
        public void CalculateUpgradeCostHelper_Contacts_should_return_correct_value_when_newPlan_has_discount_prepayment_percentage(
            int day,
            decimal currentPlanAmount,
            decimal newPlanAmount,
            int discountPlanFee,
            int totalMonth,
            int currentMonth,
            decimal total,
            decimal discountPaymentAlreadyPaid,
            decimal discountPrepaymentAmount)
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2022, 8, day));

            var newPlan = new PlanInformation
            {
                Fee = newPlanAmount,
                IdUserType = UserTypesEnum.Subscribers
            };

            var currentPlan = new UserPlan
            {
                Fee = currentPlanAmount,
                IdUserType = UserTypesEnum.Subscribers,
                TotalMonthPlan = totalMonth,
                CurrentMonthPlan = currentMonth
            };

            var currentDiscountPlan = new PlanDiscountInformation
            {
                MonthPlan = totalMonth,
                DiscountPlanFee = discountPlanFee
            };

            var newDiscountPlan = new PlanDiscountInformation
            {
                MonthPlan = totalMonth,
                DiscountPlanFee = discountPlanFee
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, newDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, currentDiscountPlan, 0, PlanTypeEnum.Marketing, null, null);

            // Assert
            Assert.Equal(total, result.Total);
            Assert.Equal(discountPaymentAlreadyPaid, result.DiscountPaymentAlreadyPaid);
            Assert.Equal(discountPrepaymentAmount, result.DiscountPrepayment.Amount);
            Assert.Equal(discountPlanFee, result.DiscountPrepayment.DiscountPercentage);
            Assert.Equal(0, result.DiscountPromocode.DiscountPercentage);
            Assert.Equal(0, result.DiscountPromocode.Amount);
        }

        #region CalculateLandingPlanAmountDetails

        [Fact]
        public void CalculateLandingPlanAmountDetails_Should_return_correct_when_has_monthly_plan_with_no_discount()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            var landingPlansSummary = new List<LandingPlanSummary>()
            {
                new LandingPlanSummary() { IdLandingPlan = 1, NumberOfPlans = 2 },
                new LandingPlanSummary() { IdLandingPlan = 2, NumberOfPlans = 0 },
                new LandingPlanSummary() { IdLandingPlan = 3, NumberOfPlans = 1 }
            };

            var landingPlansInformation = new List<LandingPlanInformation>()
            {
                new LandingPlanInformation() { PlanId = 1, PlanType = PlanTypeEnum.Landing, LandingsQty = 5, Fee = 10 },
                new LandingPlanInformation() { PlanId = 2, PlanType = PlanTypeEnum.Landing, LandingsQty = 25, Fee = 45 },
                new LandingPlanInformation() { PlanId = 3, PlanType = PlanTypeEnum.Landing, LandingsQty = 50, Fee = 80 },
            };

            var currentPlan = new UserPlan() { TotalMonthPlan = 1, CurrentMonthPlan = 0, AdditionalServices = [] };

            UserPlanInformation lastLandingPlan = null;

            var result = CalculateUpgradeCostHelper.CalculateLandingPlanAmountDetails(
                currentPlan,
                dateTimeProviderMock.Object.Now,
                landingPlansSummary,
                landingPlansInformation,
                discount: null,
                lastLandingPlan,
                firstUpgradeDate: null,
                promotion: null,
                currentPromotion: null,
                timesAppliedPromocode: null,
                billingInformation: new BillingInformation { PaymentMethod = (int)PaymentMethodEnum.CC },
                currencyRate: null);

            Assert.Equal(100, result.Total);
            Assert.Equal(100, result.NextMonthTotal);
        }

        [Fact]
        public void CalculateLandingPlanAmountDetails_Should_return_correct_when_has_monthly_plan_with_discount()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            var landingPlansSummary = new List<LandingPlanSummary>()
            {
                new LandingPlanSummary() { IdLandingPlan = 1, NumberOfPlans = 2 },
                new LandingPlanSummary() { IdLandingPlan = 2, NumberOfPlans = 0 },
                new LandingPlanSummary() { IdLandingPlan = 3, NumberOfPlans = 1 }
            };

            var landingPlansInformation = new List<LandingPlanInformation>()
            {
                new LandingPlanInformation() { PlanId = 1, PlanType = PlanTypeEnum.Landing, LandingsQty = 5, Fee = 10 },
                new LandingPlanInformation() { PlanId = 2, PlanType = PlanTypeEnum.Landing, LandingsQty = 25, Fee = 45 },
                new LandingPlanInformation() { PlanId = 3, PlanType = PlanTypeEnum.Landing, LandingsQty = 50, Fee = 80 },
            };

            var currentPlan = new UserPlan() { TotalMonthPlan = 1, CurrentMonthPlan = 0, AdditionalServices = [] };

            var discount = new PlanDiscountInformation() { DiscountPlanFee = 5, MonthPlan = 1 };

            UserPlanInformation lastLandingPlan = null;

            var result = CalculateUpgradeCostHelper.CalculateLandingPlanAmountDetails(
                currentPlan,
                dateTimeProviderMock.Object.Now,
                landingPlansSummary,
                landingPlansInformation,
                discount,
                lastLandingPlan,
                firstUpgradeDate: null,
                promotion: null,
                currentPromotion: null,
                timesAppliedPromocode: null,
                billingInformation: new BillingInformation { PaymentMethod = (int)PaymentMethodEnum.CC },
                currencyRate: null);

            Assert.Equal(95, result.Total);
            Assert.Equal(95, result.NextMonthTotal);
        }

        [Fact]
        public void CalculateLandingPlanAmountDetails_Should_return_correct_when_has_quaterly_plan_with_discount_and_start_in_first_month()
        {
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();

            var landingPlansSummary = new List<LandingPlanSummary>()
            {
                new() { IdLandingPlan = 1, NumberOfPlans = 2 },
                new() { IdLandingPlan = 2, NumberOfPlans = 0 },
                new() { IdLandingPlan = 3, NumberOfPlans = 1 }
            };

            var landingPlansInformation = new List<LandingPlanInformation>()
            {
                new() { PlanId = 1, PlanType = PlanTypeEnum.Landing, LandingsQty = 5, Fee = 10 },
                new() { PlanId = 2, PlanType = PlanTypeEnum.Landing, LandingsQty = 25, Fee = 45 },
                new() { PlanId = 3, PlanType = PlanTypeEnum.Landing, LandingsQty = 50, Fee = 80 },
            };

            var currentPlan = new UserPlan() { TotalMonthPlan = 3, CurrentMonthPlan = 0, AdditionalServices = [] };

            var discount = new PlanDiscountInformation() { DiscountPlanFee = 10, MonthPlan = 1 };

            UserPlanInformation lastLandingPlan = null;

            var result = CalculateUpgradeCostHelper.CalculateLandingPlanAmountDetails(
                currentPlan,
                dateTimeProviderMock.Object.Now,
                landingPlansSummary,
                landingPlansInformation,
                discount,
                lastLandingPlan,
                firstUpgradeDate: null,
                promotion: null,
                currentPromotion: null,
                timesAppliedPromocode: null,
                billingInformation: new BillingInformation { PaymentMethod = (int)PaymentMethodEnum.CC },
                currencyRate: null);

            Assert.Equal(90, result.Total);
            Assert.Equal(90, result.NextMonthTotal);
        }

        #endregion

        [Fact]
        public void CalculateUpgradeCostHelper_Collaborators_addon_should_discount_the_current_collaborators_plan()
        {
            var currentPlan = new UserPlan
            {
                IdUserType = UserTypesEnum.Monthly,
                TotalMonthPlan = 1,
                CurrentMonthPlan = 1,
                AdditionalServices = [new AdditionalService { IdAddOnType = (int)AddOnType.Collaborators, Fee = 15, Qty = 3 }]
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(
                new PlanInformation { ChatPlanFee = 20 },
                new PlanDiscountInformation { MonthPlan = 1, DiscountPlanFee = 0 },
                currentPlan,
                new DateTime(2021, 9, 6),
                promotion: null,
                timesAppliedPromocode: null,
                currentPromotion: null,
                firstUpgradeDate: null,
                currentDiscountPlan: new PlanDiscountInformation { MonthPlan = 1, DiscountPlanFee = 0 },
                creditsDiscount: 0,
                planType: PlanTypeEnum.Collaborators,
                billingInformation: new BillingInformation { PaymentMethod = (int)PaymentMethodEnum.CC },
                currencyRate: null);

            Assert.Equal(15, result.DiscountPaymentAlreadyPaid);
            Assert.Equal(5, result.Total);
        }

        [Fact]
        public async Task CalculateUpgradeCostWithAddOns_Collaborators_should_use_the_requested_quantity()
        {
            var accountPlansRepository = new Mock<IAccountPlansRepository>();
            accountPlansRepository
                .Setup(x => x.GetAddOnPlanInformation((int)AddOnType.Collaborators, 1))
                .ReturnsAsync(new BasePlanInformation { Fee = 10 });
            accountPlansRepository
                .Setup(x => x.GetCurrentPlanWithAdditionalServices("account@doppler.com"))
                .ReturnsAsync(new UserPlan
                {
                    IdUserType = UserTypesEnum.Monthly,
                    TotalMonthPlan = 1,
                    CurrentMonthPlan = 1,
                    AdditionalServices = [new AdditionalService { IdAddOnType = (int)AddOnType.Collaborators, Fee = 40, Qty = 2 }]
                });
            accountPlansRepository
                .Setup(x => x.GetDiscountInformation(It.IsAny<int>()))
                .ReturnsAsync(new PlanDiscountInformation { MonthPlan = 1, DiscountPlanFee = 0 });
            accountPlansRepository
                .Setup(x => x.GetFirstUpgradeDate("account@doppler.com"))
                .ReturnsAsync((DateTime?)null);

            var dateTimeProvider = new Mock<IDateTimeProvider>();
            dateTimeProvider.Setup(x => x.Now).Returns(new DateTime(2021, 9, 6));

            var userRepository = new Mock<IUserRepository>();
            userRepository
                .Setup(x => x.GetBillingInformation("account@doppler.com"))
                .ReturnsAsync(new BillingInformation { PaymentMethod = (int)PaymentMethodEnum.CC });

            var controller = new AccountPlansController(
                Mock.Of<ILogger<AccountPlansController>>(),
                accountPlansRepository.Object,
                dateTimeProvider.Object,
                Mock.Of<IPromotionRepository>(),
                userRepository.Object,
                Mock.Of<ICurrencyRepository>(),
                Mock.Of<IEncryptionService>(),
                new Doppler.AccountPlans.TimeCollector.TimeCollector());

            var response = await controller.GetCalculateUpgradeCostWithAddOns(
                "account@doppler.com",
                1,
                PlanTypeEnum.Collaborators,
                discountId: 0,
                quantity: 5);

            var result = Assert.IsType<OkObjectResult>(response);
            var amountDetails = Assert.IsType<PlanAmountDetails>(result.Value);
            Assert.Equal(50, amountDetails.PlanFee);
            Assert.Equal(40, amountDetails.DiscountPaymentAlreadyPaid);
            Assert.Equal(10, amountDetails.Total);
        }

        [Fact]
        public async Task CalculateUpgradeCostWithAddOns_Collaborators_should_require_a_positive_quantity()
        {
            var controller = new AccountPlansController(
                Mock.Of<ILogger<AccountPlansController>>(),
                Mock.Of<IAccountPlansRepository>(),
                Mock.Of<IDateTimeProvider>(),
                Mock.Of<IPromotionRepository>(),
                Mock.Of<IUserRepository>(),
                Mock.Of<ICurrencyRepository>(),
                Mock.Of<IEncryptionService>(),
                new Doppler.AccountPlans.TimeCollector.TimeCollector());

            var response = await controller.GetCalculateUpgradeCostWithAddOns(
                "account@doppler.com",
                1,
                PlanTypeEnum.Collaborators,
                discountId: 0);

            Assert.IsType<BadRequestObjectResult>(response);
        }
    }
}
