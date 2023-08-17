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
    public class GetPlanInformationTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TokenExpire20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        private readonly WebApplicationFactory<Startup> _factory;

        public GetPlanInformationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_PlanInformation_method_should_get_right_values_when_plan_id_is_valid()
        {
            // Arrange
            const string expectedContent = "{\"emailQty\":100000,\"fee\":180,\"subscribersQty\":0,\"type\":\"STANDARD\",\"currentMonthPlan\":0,\"idUserType\":0,\"discountPlanFeeAdmin\":0,\"chatPlanFee\":null,\"chatPlanConversationQty\":null}";

            var planRenewalInformation = new PlanInformation
            {
                EmailQty = 100000,
                Fee = 180,
                SubscribersQty = 0,
                Type = "STANDARD",
                DiscountPlanFeeAdmin = 0
            };

            var mockConnection = new Mock<DbConnection>();
            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<PlanInformation>(null, null, null, null, null))
                .ReturnsAsync(planRenewalInformation);


            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/plans/1/")
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
