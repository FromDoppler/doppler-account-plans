using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Doppler.AccountPlans
{
    public class GetCalculateUpgradeLandingPlanCostTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TokenExpire20330518 =
    "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";

        private readonly WebApplicationFactory<Startup> _factory;

        public GetCalculateUpgradeLandingPlanCostTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_should_return_OK_StatusCode()
        {
            var userAccount = "test1@example.com";
            var idDiscountPlan = 1;
            var acccountPlansRepository = new Mock<IAccountPlansRepository>();
            var promotionRepository = new Mock<IPromotionRepository>();

            acccountPlansRepository
                .Setup(x => x.GetCurrentPlanWithAdditionalServices(userAccount))
                .ReturnsAsync(new UserPlan()
                {
                    IdDiscountPlan = idDiscountPlan,
                    AdditionalServices = [new AdditionalService { IdAddOnType = 1 }]
                });

            acccountPlansRepository
                .Setup(x => x.GetLandingPlans())
                .ReturnsAsync(new List<LandingPlanInformation>());

            acccountPlansRepository
                .Setup(x => x.GetDiscountInformation(idDiscountPlan))
                .ReturnsAsync(new PlanDiscountInformation());

            promotionRepository
                .Setup(x => x.GetCurrentPromotionByAccountName(userAccount))
                .ReturnsAsync((Promotion)null);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(acccountPlansRepository.Object);
                    services.AddSingleton(promotionRepository.Object);
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, $"accounts/{userAccount}/newplan/landingplan/calculate?landingIds=1,2&landingPacks=1,2")
            {
                Headers = { { "Authorization", $"Bearer {TokenExpire20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task GET_should_return_badrequest_when_landingIds_queryparam_is_not_defined()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "accounts/test1@example.com/newplan/landingplan/calculate?landingPacks=1,2")
            {
                Headers = { { "Authorization", $"Bearer {TokenExpire20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_should_return_badrequest_when_landingPacks_queryparam_is_not_defined()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "accounts/test1@example.com/newplan/landingplan/calculate?landingIds=1,2")
            {
                Headers = { { "Authorization", $"Bearer {TokenExpire20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_should_return_badrequest_when_landingPacks_and_landingIds_queryparam_are_not_defined()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

            var request = new HttpRequestMessage(HttpMethod.Get, "accounts/test1@example.com/newplan/landingplan/calculate")
            {
                Headers = { { "Authorization", $"Bearer {TokenExpire20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_should_return_badrequest_when_the_user_has_no_billingcredit()
        {
            var userAccount = "test1@example.com";
            var acccountPlansRepository = new Mock<IAccountPlansRepository>();

            acccountPlansRepository
                .Setup(x => x.GetCurrentPlanInformation(userAccount))
                .ReturnsAsync(() => null);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(acccountPlansRepository.Object);
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "accounts/test1@example.com/newplan/landingplan/calculate?landingIds=1,2&landingPacks=1,2")
            {
                Headers = { { "Authorization", $"Bearer {TokenExpire20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GET_should_return_badrequest_when_the_number_of_landingPacks_and_landingIds_queryparam_are_not_equal()
        {
            var userAccount = "test1@example.com";
            var idDiscountPlan = 1;
            var acccountPlansRepository = new Mock<IAccountPlansRepository>();

            acccountPlansRepository
                .Setup(x => x.GetCurrentPlanInformation(userAccount))
                .ReturnsAsync(new UserPlanInformation() { IdDiscountPlan = idDiscountPlan });

            acccountPlansRepository
                .Setup(x => x.GetLandingPlans())
                .ReturnsAsync(new List<LandingPlanInformation>());

            acccountPlansRepository
                .Setup(x => x.GetDiscountInformation(idDiscountPlan))
                .ReturnsAsync(new PlanDiscountInformation());

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(acccountPlansRepository.Object);
                });
            }).CreateClient(new WebApplicationFactoryClientOptions());

            // In the query params are 3 landings IDs and 2 landings packs
            var request = new HttpRequestMessage(HttpMethod.Get, $"accounts/{userAccount}/newplan/landingplan/calculate?landingIds=1,2,3&landingPacks=1,2")
            {
                Headers = { { "Authorization", $"Bearer {TokenExpire20330518}" } }
            };

            // Act
            var response = await client.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
