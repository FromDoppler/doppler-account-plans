using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Doppler.AccountPlans.Encryption;
using Doppler.AccountPlans.Infrastructure;
using Doppler.AccountPlans.Model;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace Doppler.AccountPlans
{
    public class GetPromoCodeTest : IClassFixture<WebApplicationFactory<Startup>>
    {
        private const string TokenExpire20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJleHAiOjIwMDAwMDAwMDB9.rUtvRqMxrnQzVHDuAjgWa2GJAJwZ-wpaxqdjwP7gmVa7XJ1pEmvdTMBdirKL5BJIE7j2_hsMvEOKUKVjWUY-IE0e0u7c82TH0l_4zsIztRyHMKtt9QE9rBRQnJf8dcT5PnLiWkV_qEkpiIKQ-wcMZ1m7vQJ0auEPZyyFBKmU2caxkZZOZ8Kw_1dx-7lGUdOsUYad-1Rt-iuETGAFijQrWggcm3kV_KmVe8utznshv2bAdLJWydbsAUEfNof0kZK5Wu9A80DJd3CRiNk8mWjQxF_qPOrGCANOIYofhB13yuYi48_8zVPYku-llDQjF77BmQIIIMrCXs8IMT3Lksdxuw";
        private readonly WebApplicationFactory<Startup> _factory;

        public GetPromoCodeTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_PromoCode_Information_should_get_right_values_when_promoCode_is_expired()
        {
            // Arrange
            const string expectedContent = "{\"code\":\"test\",\"canApply\":false,\"expiredPromocode\":true,\"promotionApplied\":null,\"planPromotions\":[]}";
            IList<Promotion> promotions =
            [
                new Promotion {
                    ExtraCredits = 1,
                    DiscountPercentage = 2,
                    IdPromotion = 3,
                    ExpirationDate = new System.DateTime(2026,2,24)
                }
            ];

            var promoCodeRepositoryMock = new Mock<IPromotionRepository>();
            promoCodeRepositoryMock.Setup(x => x.GetPromotionsByCode(It.IsAny<string>()))
                .ReturnsAsync(promotions);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(promoCodeRepositoryMock.Object);
                    services.AddSingleton(Mock.Of<IAccountPlansRepository>());
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/plans/1/validate/test")
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
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task GET_PromoCode_Information_should_get_right_values_when_promoCode_is_valid()
        {
            // Arrange
            const string expectedContent = "{\"code\":\"test\",\"canApply\":true,\"expiredPromocode\":false,\"promotionApplied\":{\"idPromotion\":3,\"idUserTypePlan\":null,\"idUserType\":null,\"extraCredits\":1,\"discountPercentage\":2,\"duration\":null,\"code\":null,\"allPlans\":false,\"allSubscriberPlans\":false,\"allPrepaidPlans\":false,\"allMonthlyPlans\":false,\"active\":false,\"idAddOnPlan\":null,\"idAddOnType\":null,\"quantity\":null,\"canApply\":false,\"expirationDate\":null},\"planPromotions\":[]}";
            IList<Promotion> promotions =
            [
                new Promotion {
                    ExtraCredits = 1,
                    DiscountPercentage = 2,
                    IdPromotion = 3,
                    ExpirationDate = null
                }
            ];

            var promoCodeRepositoryMock = new Mock<IPromotionRepository>();
            promoCodeRepositoryMock.Setup(x => x.GetPromotionByCodeAndPlanId(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Promotion
                {
                    ExtraCredits = 1,
                    DiscountPercentage = 2,
                    IdPromotion = 3
                });
            promoCodeRepositoryMock.Setup(x => x.GetPromotionsByCode(It.IsAny<string>()))
                .ReturnsAsync(promotions);

            var accountPlansRepositoryMock = new Mock<IAccountPlansRepository>();
            accountPlansRepositoryMock.Setup(x => x.GetPlanInformation(It.IsAny<int>()))
                .ReturnsAsync(new PlanInformation
                {
                    IdUserType = Enums.UserTypesEnum.Subscribers,
                    SubscribersQty = 500

                });

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(promoCodeRepositoryMock.Object);
                    services.AddSingleton(accountPlansRepositoryMock.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/plans/1/validate/test")
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(expectedContent, responseContent);
        }

        [Fact]
        public async Task GET_PromoCode_Information_should_return_not_found_when_promoCode_not_exist()
        {
            // Arrange
            var promoCodeRepositoryMock = new Mock<IPromotionRepository>();
            promoCodeRepositoryMock.Setup(x => x.GetPromotionsByCode(It.IsAny<string>()))
                .ReturnsAsync(new List<Promotion>());
            promoCodeRepositoryMock.Setup(x => x.GetPromotionByCodeAndPlanId(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(null as Promotion);

            var client = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddSingleton(promoCodeRepositoryMock.Object);
                    services.AddSingleton(Mock.Of<IEncryptionService>());
                });

            }).CreateClient(new WebApplicationFactoryClientOptions());

            var request = new HttpRequestMessage(HttpMethod.Get, "/plans/1/validate/test")
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
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
    }
}
