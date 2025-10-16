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
        public async Task GET_PromoCode_Information_should_get_right_values_when_promoCode_is_valid()
        {
            // Arrange
            const string expectedContent = "{\"idPromotion\":3,\"extraCredits\":1,\"discountPercentage\":2,\"duration\":null,\"code\":null,\"active\":false,\"idAddOnPlan\":null}";
            var promoCodeRepositoryMock = new Mock<IPromotionRepository>();
            promoCodeRepositoryMock.Setup(x => x.GetPromotionByCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .ReturnsAsync(new Promotion
                {
                    ExtraCredits = 1,
                    DiscountPercentage = 2,
                    IdPromotion = 3
                });

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
            promoCodeRepositoryMock.Setup(x => x.GetPromotionByCode(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
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
