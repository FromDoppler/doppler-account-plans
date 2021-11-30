using System.Collections.Generic;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Dapper;
using Doppler.AccountPlans.Encryption;
using Doppler.AccountPlans.Enums;
using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using Doppler.AccountPlans.Test.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Dapper;
using Xunit;

namespace Doppler.AccountPlans
{
    public class GetCalculateUpgradeCostTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TokenAccount123Test1AtTestDotComExpire20330518 =
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJpYXQiOjE1ODQ0NjM1MjksImV4cCI6MTY0NTU2NzUyOX0.eJZYYKZOw439rWO98urRkmR4H9UqhVoD870RECWtOQlsUEpTeNPFgQfz-bQHyYZ73lkNunvmW4s31wJB1BFrVr4JqklhvM0zEem70aGDrxa1WwNB268p-dzGZqG_RAU4k7oW1JwSvrg6DGVQTlP2DyeNvyO1qwpt8m0yS4z6x3jTzn4V-6iEOIBMp2DxwzMU6x03e_I4jZJRal2T9941BoM88c2S-IXWR_uS0AGbH1Yl7ZMUSx7wQEZi2o7UpOefR6TpKkOTj5iSeSMSVvpGvOjKP5Lk_Y7TSLpNEKTcMaahA1DK4R_EYAbtiIWIvSscEqxlWBzo7AB1MQdNbqrVlA";

        private readonly WebApplicationFactory<Startup> _factory;

        public GetCalculateUpgradeCostTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_calculateUpgradeCost_method_should_get_right_values_when_plan_and_are_valid()
        {
            var mockConnection = new Mock<DbConnection>();

            mockConnection.SetupDapperAsync(x => x.QueryAsync<PlanInformation>(null, null, null, null, null))
                .ReturnsAsync(new List<PlanInformation>
                {
                    new PlanInformation
                    {
                        Fee = 8,
                        EmailQty = 1003
                    }

                });

            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<PlanDiscountInformation>(null, null, null, null, null))
                .ReturnsAsync(new PlanDiscountInformation
                {
                    MonthPlan = 1
                });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                    services.AddSingleton(Mock.Of<IPromotionRepository>());
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/accounts/1/newplan/1/calculate")
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

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_calculateUpgradeCost_method_should_get_right_values_when_new_plan_isPrepaid()
        {
            var mockConnection = new Mock<DbConnection>();

            mockConnection.SetupDapperAsync(x => x.QueryAsync<PlanInformation>(null, null, null, null, null))
                .ReturnsAsync(new List<PlanInformation>
                {
                    new PlanInformation
                    {
                        Fee = 8,
                        EmailQty = 1003,
                        IdUserType = UserTypesEnum.Individual
                    }

                });

            mockConnection.SetupDapperAsync(c => c.QueryFirstOrDefaultAsync<PlanDiscountInformation>(null, null, null, null, null))
                .ReturnsAsync(new PlanDiscountInformation
                {
                    MonthPlan = 1
                });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.SetupConnectionFactory(mockConnection.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                    services.AddSingleton(Mock.Of<IPromotionRepository>());
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/accounts/1/newplan/1/calculate")
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

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
