using System.Net;
using System.Net.Http;
using System.Threading;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Moq.Protected;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.Services;

public class WhenCheckingExternalWebsiteHealth
{
    [TestCase("ftp")]
    [TestCase("ssh")]
    [TestCase("sftp")]
    [TestCase("mailto")]
    public async Task Invalid_Schemes_Are_Not_Accepted(string scheme)
    {
        // arrange
        var externalWebsiteMessageHandler = new Mock<HttpMessageHandler>();
        externalWebsiteMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.OK));
        
        var sut = new ExternalWebsiteHealthCheckService(new HttpClient(externalWebsiteMessageHandler.Object));
        var action = async () => await sut.IsHealthyAsync(new Uri($"{scheme}://www.example.com"), CancellationToken.None);
        
        // act/assert
        await action.Should().ThrowAsync<InvalidSchemeException>();
    }
    
    [TestCase("http")]
    [TestCase("https")]
    public async Task Valid_Schemes_Are_Accepted(string scheme)
    {
        // arrange
        var externalWebsiteMessageHandler = new Mock<HttpMessageHandler>();
        externalWebsiteMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(HttpStatusCode.OK));
        
        var sut = new ExternalWebsiteHealthCheckService(new HttpClient(externalWebsiteMessageHandler.Object));
        var action = async () => await sut.IsHealthyAsync(new Uri($"{scheme}://www.example.com"), CancellationToken.None);
        
        // act/assert
        await action.Should().NotThrowAsync<InvalidSchemeException>();
    }
    
    [TestCase(HttpStatusCode.OK)]
    [TestCase(HttpStatusCode.Accepted)]
    public async Task Then_Websites_Are_Valid_If_Success_Response_Is_Returned(HttpStatusCode statusCode)
    {
        // arrange
        var externalWebsiteMessageHandler = new Mock<HttpMessageHandler>();
        externalWebsiteMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(statusCode));
        
        var sut = new ExternalWebsiteHealthCheckService(new HttpClient(externalWebsiteMessageHandler.Object));

        // act
        var result = await sut.IsHealthyAsync(new Uri("https://www.example.com"), CancellationToken.None);
        
        // act/assert
        result.Should().BeTrue();
    }
    
    [TestCase(HttpStatusCode.NotFound)]
    [TestCase(HttpStatusCode.BadGateway)]
    [TestCase(HttpStatusCode.BadRequest)]
    [TestCase(HttpStatusCode.Unauthorized)]
    [TestCase(HttpStatusCode.Ambiguous)]
    public async Task Then_Websites_Are_Invalid_If_Failure_Response_Is_Returned(HttpStatusCode statusCode)
    {
        // arrange
        var externalWebsiteMessageHandler = new Mock<HttpMessageHandler>();
        externalWebsiteMessageHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage _, CancellationToken _) => new HttpResponseMessage(statusCode));
        
        var sut = new ExternalWebsiteHealthCheckService(new HttpClient(externalWebsiteMessageHandler.Object));

        // act
        var result = await sut.IsHealthyAsync(new Uri("https://www.example.com"), CancellationToken.None);
        
        // act/assert
        result.Should().BeFalse();
    }
}