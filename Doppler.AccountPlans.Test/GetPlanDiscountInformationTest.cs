using Dapper;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Dapper;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.AccountPlans
{
    public class GetPlanDiscountInformationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TokenAccount123Test1AtTestDotComExpire20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJpYXQiOjE1ODQ0NjM1MjksImV4cCI6MTY0NTU2NzUyOX0.eJZYYKZOw439rWO98urRkmR4H9UqhVoD870RECWtOQlsUEpTeNPFgQfz-bQHyYZ73lkNunvmW4s31wJB1BFrVr4JqklhvM0zEem70aGDrxa1WwNB268p-dzGZqG_RAU4k7oW1JwSvrg6DGVQTlP2DyeNvyO1qwpt8m0yS4z6x3jTzn4V-6iEOIBMp2DxwzMU6x03e_I4jZJRal2T9941BoM88c2S-IXWR_uS0AGbH1Yl7ZMUSx7wQEZi2o7UpOefR6TpKkOTj5iSeSMSVvpGvOjKP5Lk_Y7TSLpNEKTcMaahA1DK4R_EYAbtiIWIvSscEqxlWBzo7AB1MQdNbqrVlA";
        private readonly WebApplicationFactory<Startup> _factory;

        public GetPlanDiscountInformationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_PlanDiscountInformation_method_should_get_right_values_when_plan_and_paymentmethod_are_valid()
        {
            // Arrange
            const string expectedContent = "[{\"idDiscountPlan\":\"1\",\"discountPlanFee\":\"0\",\"monthPlan\":\"1\"},{\"idDiscountPlan\":\"2\",\"discountPlanFee\":\"5\",\"monthPlan\":\"3\"},{\"idDiscountPlan\":\"3\",\"discountPlanFee\":\"15\",\"monthPlan\":\"6\"},{\"idDiscountPlan\":\"4\",\"discountPlanFee\":\"25\",\"monthPlan\":\"12\"}]";

            var planRenewalInformation = new[] { new PlanDiscountInformation
            {
                IdDiscountPlan = "1",
                DiscountPlanFee = "0",
                MonthPlan = "1"
            }, new PlanDiscountInformation
            {
                IdDiscountPlan = "2",
                DiscountPlanFee = "5",
                MonthPlan = "3"
            }, new PlanDiscountInformation
            {
                IdDiscountPlan = "3",
                DiscountPlanFee = "15",
                MonthPlan = "6"
            }, new PlanDiscountInformation
            {
                IdDiscountPlan = "4",
                DiscountPlanFee = "25",
                MonthPlan = "12"
            } };

            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupDapperAsync(c => c.QueryAsync<PlanDiscountInformation>(null, null, null, null, null))
                .ReturnsAsync(planRenewalInformation);


            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/plans/1/CC/discounts")
            {
                Headers =
                {
                    {
                        "Authorization", $"Bearer {TokenAccount123Test1AtTestDotComExpire20330518}"
                    }
                }
            };

            // Act
            var response = await client.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            Assert.Equal(expectedContent, responseContent);
        }

    }
}