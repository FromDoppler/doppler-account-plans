using System;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.RenewalHandlers;
using Doppler.AccountPlans.Utils;
using Moq;
using Xunit;

namespace Doppler.AccountPlans
{
    public class MonthlyHandlerTest
    {
        [Fact]
        public void Monthly_handle_calculateUpgradeCost_method_should_return_correct_value_when_day_is_minor_that_21()
        {
            // Arrange
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 19));
            var monthlyHandler = new MonthlyHandler(dateTimeProviderMock.Object);

            var newPlan = new PlanInformation
            {
                Fee = 5
            };
            var currentPlan = new PlanInformation
            {
                Fee = 2
            };
            var planDiscount = new PlanDiscountInformation
            {
                DiscountPlanFee = 0
            };

            // Act
            var planAmountDetails = monthlyHandler.CalculatePlanAmountDetails(newPlan, planDiscount, currentPlan);

            // Assert
            Assert.Equal(2, planAmountDetails.Total);
            Assert.Equal(3, planAmountDetails.DiscountPaymentAlreadyPaid);
        }

        [Fact]
        public void Monthly_handle_calculateUpgradeCost_method_should_return_correct_value_when_day_is_major_that_21()
        {
            // Arrange
            var dateTimeProviderMock = new Mock<IDateTimeProvider>();
            dateTimeProviderMock.Setup(x => x.Now)
                .Returns(new DateTime(2021, 9, 21));
            var monthlyHandler = new MonthlyHandler(dateTimeProviderMock.Object);

            var newPlan = new PlanInformation
            {
                Fee = 5
            };
            var currentPlan = new PlanInformation
            {
                Fee = 2
            };
            var planDiscount = new PlanDiscountInformation
            {
                DiscountPlanFee = 0
            };

            // Act
            var planAmountDetails = monthlyHandler.CalculatePlanAmountDetails(newPlan, planDiscount, currentPlan);

            // Assert
            Assert.Equal(5, planAmountDetails.Total);
            Assert.Equal(0, planAmountDetails.DiscountPaymentAlreadyPaid);
        }
    }
}
