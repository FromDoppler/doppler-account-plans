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
        private const string TokenExpire20330518 =
            "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";

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
                        "Authorization", $"Bearer {TokenExpire20330518}"
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
                        "Authorization", $"Bearer {TokenExpire20330518}"
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
