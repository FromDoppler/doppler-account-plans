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
        private const string TokenAccount123Test1AtTestDotComExpire20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJpYXQiOjE1ODQ0NjM1MjksImV4cCI6MTY0NTU2NzUyOX0.eJZYYKZOw439rWO98urRkmR4H9UqhVoD870RECWtOQlsUEpTeNPFgQfz-bQHyYZ73lkNunvmW4s31wJB1BFrVr4JqklhvM0zEem70aGDrxa1WwNB268p-dzGZqG_RAU4k7oW1JwSvrg6DGVQTlP2DyeNvyO1qwpt8m0yS4z6x3jTzn4V-6iEOIBMp2DxwzMU6x03e_I4jZJRal2T9941BoM88c2S-IXWR_uS0AGbH1Yl7ZMUSx7wQEZi2o7UpOefR6TpKkOTj5iSeSMSVvpGvOjKP5Lk_Y7TSLpNEKTcMaahA1DK4R_EYAbtiIWIvSscEqxlWBzo7AB1MQdNbqrVlA";
        private readonly WebApplicationFactory<Startup> _factory;

        public GetPlanInformationTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_PlanInformation_method_should_get_right_values_when_plan_id_is_valid()
        {
            // Arrange
            const string expectedContent = "{\"emailQty\":100000,\"fee\":180,\"subscribersQty\":0,\"type\":\"STANDARD\",\"currentMonthPlan\":0}";

            var planRenewalInformation = new PlanInformation
            {
                EmailQty = 100000,
                Fee = 180,
                SubscribersQty = 0,
                Type = "STANDARD"
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
