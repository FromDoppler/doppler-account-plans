using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;
using Moq;
using System;
using Xunit;

namespace Doppler.AccountPlans
{
    public class CalculateUpgradeCostTest
    {
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(5, result.Total);
            Assert.Equal(0, result.DiscountPaymentAlreadyPaid);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(9, result.Total);
            Assert.Equal(6, result.DiscountPaymentAlreadyPaid);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(6, result.Total);
            Assert.Equal(4, result.DiscountPaymentAlreadyPaid);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 2
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);
            // Assert
            Assert.Equal(13, result.Total);
            Assert.Equal(10, result.DiscountPaymentAlreadyPaid);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 2
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(10, result.Total);
            Assert.Equal(8, result.DiscountPaymentAlreadyPaid);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 4
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(20, result.Total);
            Assert.Equal(18, result.DiscountPaymentAlreadyPaid);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 4
            };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(18, result.Total);
            Assert.Equal(16, result.DiscountPaymentAlreadyPaid);
        }
    }
}
