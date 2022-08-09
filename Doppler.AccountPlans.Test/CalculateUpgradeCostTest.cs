using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;
using Moq;
using System;
using Doppler.AccountPlans.Enums;
using Xunit;

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

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, null, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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
            };

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 2
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);
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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 2
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 4
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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
            };

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 4
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 4,
                IdUserType = UserTypesEnum.Individual
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, null, currentPlan, dateTimeProviderMock.Object.Now, new Promotion(), null, null, null, null);

            // Assert
            Assert.Equal(0, result.Total);
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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                IdUserType = UserTypesEnum.Individual
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, null, currentPlan, dateTimeProviderMock.Object.Now, new Promotion(), null, null, null, null);

            // Assert
            Assert.Equal(0, result.Total);
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

            var currentPlan = new UserPlanInformation
            {
                Fee = 2,
                IdUserType = UserTypesEnum.Individual,
                TotalMonthPlan = 1
            };

            var promotion = new Promotion
            {
                DiscountPercentage = discountPercentage
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, null, currentPlan, dateTimeProviderMock.Object.Now, promotion, null, null, null, null);

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

            var currentPlan = new UserPlanInformation
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

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, newDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now, null, null, null, null, currentDiscountPlan);

            // Assert
            Assert.Equal(total, result.Total);
            Assert.Equal(discountPaymentAlreadyPaid, result.DiscountPaymentAlreadyPaid);
            Assert.Equal(discountPrepaymentAmount, result.DiscountPrepayment.Amount);
            Assert.Equal(discountPlanFee, result.DiscountPrepayment.DiscountPercentage);
            Assert.Equal(0, result.DiscountPromocode.DiscountPercentage);
            Assert.Equal(0, result.DiscountPromocode.Amount);
        }
    }
}
