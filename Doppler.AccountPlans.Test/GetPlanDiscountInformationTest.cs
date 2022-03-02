using Dapper;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Moq;
using Moq.Dapper;
using System.Data.Common;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.AccountPlans
{
    public class GetPlanDiscountInformationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TokenExpire20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        private readonly WebApplicationFactory<Startup> _factory;

        public GetPlanDiscountInformationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_PlanDiscountInformation_method_should_get_right_values_when_plan_and_paymentmethod_are_valid()
        {
            // Arrange
            const string expectedContent = "[{\"idDiscountPlan\":\"1\",\"discountPlanFee\":0,\"monthPlan\":1,\"applyPromo\":true},{\"idDiscountPlan\":\"2\",\"discountPlanFee\":5,\"monthPlan\":3,\"applyPromo\":false},{\"idDiscountPlan\":\"3\",\"discountPlanFee\":15,\"monthPlan\":6,\"applyPromo\":false},{\"idDiscountPlan\":\"4\",\"discountPlanFee\":25,\"monthPlan\":12,\"applyPromo\":false}]";

            var planRenewalInformation = new[] { new PlanDiscountInformation
            {
                IdDiscountPlan = "1",
                DiscountPlanFee = 0,
                MonthPlan = 1,
                ApplyPromo = true
            }, new PlanDiscountInformation
            {
                IdDiscountPlan = "2",
                DiscountPlanFee = 5,
                MonthPlan = 3,
                ApplyPromo = false
            }, new PlanDiscountInformation
            {
                IdDiscountPlan = "3",
                DiscountPlanFee = 15,
                MonthPlan = 6,
                ApplyPromo = false
            }, new PlanDiscountInformation
            {
                IdDiscountPlan = "4",
                DiscountPlanFee = 25,
                MonthPlan = 12,
                ApplyPromo = false
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
                        "Authorization", $"Bearer {TokenExpire20330518}"
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
