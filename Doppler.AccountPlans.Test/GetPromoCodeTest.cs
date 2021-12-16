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
        private const string TokenAccount123Test1AtTestDotComExpire20330518 = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc1NVIjp0cnVlLCJpYXQiOjE1ODQ0NjM1MjksImV4cCI6MTY0NTU2NzUyOX0.eJZYYKZOw439rWO98urRkmR4H9UqhVoD870RECWtOQlsUEpTeNPFgQfz-bQHyYZ73lkNunvmW4s31wJB1BFrVr4JqklhvM0zEem70aGDrxa1WwNB268p-dzGZqG_RAU4k7oW1JwSvrg6DGVQTlP2DyeNvyO1qwpt8m0yS4z6x3jTzn4V-6iEOIBMp2DxwzMU6x03e_I4jZJRal2T9941BoM88c2S-IXWR_uS0AGbH1Yl7ZMUSx7wQEZi2o7UpOefR6TpKkOTj5iSeSMSVvpGvOjKP5Lk_Y7TSLpNEKTcMaahA1DK4R_EYAbtiIWIvSscEqxlWBzo7AB1MQdNbqrVlA";
        private readonly WebApplicationFactory<Startup> _factory;

        public GetPromoCodeTest(WebApplicationFactory<Startup> factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task GET_PromoCode_Information_should_get_right_values_when_promoCode_is_valid()
        {
            // Arrange
            const string expectedContent = "{\"idPromotion\":3,\"extraCredits\":1,\"discountPercentage\":2}";
            var promoCodeRepositoryMock = new Mock<IPromotionRepository>();
            promoCodeRepositoryMock.Setup(x => x.GetPromotionByCode(It.IsAny<string>(), It.IsAny<int>()))
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
                        "Authorization", $"Bearer {TokenAccount123Test1AtTestDotComExpire20330518}"
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
            promoCodeRepositoryMock.Setup(x => x.GetPromotionByCode(It.IsAny<string>(), It.IsAny<int>()))
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
                        "Authorization", $"Bearer {TokenAccount123Test1AtTestDotComExpire20330518}"
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
