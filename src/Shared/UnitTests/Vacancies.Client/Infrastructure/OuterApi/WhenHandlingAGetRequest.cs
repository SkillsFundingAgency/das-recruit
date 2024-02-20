using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi
{
    public class WhenHandlingAGetRequest
    {
        [Fact]
        public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Returned()
        {
            //Arrange
            var key = "123-abc-567";
            var getTestRequest = new GetTestRequest();
            var testObject = new List<string>();
            var config = new OuterApiConfiguration {BaseUrl = "http://valid-url/", Key = key};
            var mockConfig = new Mock<IOptions<OuterApiConfiguration>>();
            mockConfig.Setup(x => x.Value).Returns(config);

            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(testObject)),
                StatusCode = HttpStatusCode.Accepted
            };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, $"{config.BaseUrl}{getTestRequest.GetUrl}", config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, mockConfig.Object);

            //Act
            var actual = await apiClient.Get<List<string>>(getTestRequest);
            
            //Assert
            actual.Should().BeEquivalentTo(testObject);
        }
        
        [Fact]
        public async Task Then_If_It_Is_Not_Successful_An_Exception_Is_Thrown()
        {
            //Arrange
            var key = "123-abc-567";
            var getTestRequest = new GetTestRequest();
            var config = new OuterApiConfiguration {BaseUrl = "http://valid-url/", Key = key };
            var mockConfig = new Mock<IOptions<OuterApiConfiguration>>();
            mockConfig.Setup(x => x.Value).Returns(config);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.BadRequest
            };
            
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, $"{config.BaseUrl}{getTestRequest.GetUrl}", config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, mockConfig.Object);

            //Act Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => apiClient.Get<List<string>>(getTestRequest));
            
        }

        [Fact]
        public async Task Then_If_It_Not_Found_Default_Object_Is_Returned()
        {
            //Arrange
            var key = "123-abc-567";
            var getTestRequest = new GetTestRequest();
            var config = new OuterApiConfiguration { BaseUrl = "http://valid-url/", Key = key };
            var mockConfig = new Mock<IOptions<OuterApiConfiguration>>();
            mockConfig.Setup(x => x.Value).Returns(config);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(""),
                StatusCode = HttpStatusCode.NotFound
            };

            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, $"{config.BaseUrl}{getTestRequest.GetUrl}", config.Key);
            var client = new HttpClient(httpMessageHandler.Object);
            var apiClient = new OuterApiClient(client, mockConfig.Object);

            //Act
            var result = await apiClient.Get<GetGeoPointResponse>(getTestRequest);

            // Assert
            result.Should().BeNull();
        }

        public class GetTestRequest : IGetApiRequest
        {
            public string GetUrl => "test-url/get";
        }
    }
    public static class MessageHandler
    {
        public static Mock<HttpMessageHandler> SetupMessageHandlerMock(HttpResponseMessage response, string url, string key)
        {
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(c =>
                        c.Method.Equals(HttpMethod.Get)
                        && c.Headers.Contains("Ocp-Apim-Subscription-Key")
                        && c.Headers.GetValues("Ocp-Apim-Subscription-Key").First().Equals(key)
                        && c.Headers.Contains("X-Version")
                        && c.Headers.GetValues("X-Version").First().Equals("1")
                        && c.RequestUri.AbsoluteUri.Equals(url)),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync((HttpRequestMessage request, CancellationToken token) => response);
            return httpMessageHandler;
        }
    }
    
}