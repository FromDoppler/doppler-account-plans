using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Utils;
using Moq;
using System;
using System.Collections.Generic;
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

            PlanInformation currentPlan = null;

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

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

            PlanInformation currentPlan = null;

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(14, result.Total);
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

            PlanInformation currentPlan = null;

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(25, result.Total);
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

            PlanInformation currentPlan = null;

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

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
                CurrentMonthPlan = 0
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(5, result.Total);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(5, result.Total);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(12, result.Total);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(23, result.Total);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 0
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(43, result.Total);
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

            var currentPlan = new PlanInformation
            {
                Fee = 2,
                CurrentMonthPlan = 1
            };

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(8, result.Total);
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

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(10, result.Total);
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

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);
            // Assert
            Assert.Equal(15, result.Total);
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

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(17, result.Total);
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

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(27, result.Total);
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

            var taxesSettings = new List<TaxSettings>();

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(29, result.Total);
            Assert.Equal(16, result.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_with_percentage_in_tax_should_return_correct_amount_for_the_taxes()
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

            var taxesSettings = new List<TaxSettings> { new TaxSettings { Percentage = 21 } };

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, taxesSettings, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(1, result.Taxes.Count);
            Assert.Equal(6.09m, result.Taxes[0].Amount);
        }

        [Fact]
        public void CalculateUpgradeCostHelper_without_taxes_setting_should_return_empty_list_for_the_taxes()
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

            var result = CalculateUpgradeCostHelper.CalculatePlanAmountDetails(newPlan, currentDiscountPlan, currentPlan, null, dateTimeProviderMock.Object.Now);

            // Assert
            Assert.Equal(0, result.Taxes.Count);
        }
    }
}
