using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.OuterApi;

public class WhenHandlingAPostRequest
{
    [Fact]
    public async Task Then_The_Endpoint_Is_Called_With_Authentication_Header_And_Data_Returned()
    {
        //Arrange
        var key = "123-abc-567";
        var testObject = new List<string>{"test-value"};
        var postTestRequest = new PostTestRequest(testObject);
            
        var config = new OuterApiConfiguration {BaseUrl = "http://valid-url/", Key = key};
        var mockConfig = new Mock<IOptions<OuterApiConfiguration>>();
        mockConfig.Setup(x => x.Value).Returns(config);

        var response = new HttpResponseMessage
        {
            Content = new StringContent(JsonConvert.SerializeObject(testObject)),
            StatusCode = HttpStatusCode.Accepted
        };
        var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, $"{config.BaseUrl}{postTestRequest.PostUrl}", config.Key,HttpMethod.Post);
        var client = new HttpClient(httpMessageHandler.Object);
        var apiClient = new OuterApiClient(client, mockConfig.Object);

        //Act
        await apiClient.Post(postTestRequest);
            
        //Assert
        httpMessageHandler.Protected()
            .Verify<Task<HttpResponseMessage>>(
                "SendAsync", Times.Once(),
                ItExpr.Is<HttpRequestMessage>(c =>
                    c.Method.Equals(HttpMethod.Post)
                    && c.Content.ReadAsStringAsync().Result.Contains("test-value")),
                ItExpr.IsAny<CancellationToken>()
            );
    }
        
    public class PostTestRequest(List<string> data) : IPostApiRequest
    {
        public string PostUrl => "test-url/get";
        public object Data { get; set; } = data;
    }
}