using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Recruit.Vacancies.Client.Application.Services
{
    public class GetTrainingProviderDetailsTest
    {
        private readonly Mock<IOptions<OuterApiConfiguration>> _mockOptions;
        private GetTrainingProviderDetails _getTrainingProviderDetails;

        public GetTrainingProviderDetailsTest()
        {
            _mockOptions = new Mock<IOptions<OuterApiConfiguration>>();
        }

        [Fact]
        public async Task Then_The_Endpoint_Is_Called_And_Error_Content_Returned_If_Error()
        {
            //arrange
            var appSettings = new OuterApiConfiguration
            {
                BaseUrl = "https://test.local"
            };
            _mockOptions.Setup(x => x.Value).Returns(appSettings);
            var response = new HttpResponseMessage
            {
                Content = new StringContent(JsonConvert.SerializeObject(new GetProviderResponse()), Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.InternalServerError
            };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, appSettings.BaseUrl);
            var client = new HttpClient(httpMessageHandler.Object);
            var outerApiClient = new OuterApiClient(client, _mockOptions.Object);

            _getTrainingProviderDetails = new GetTrainingProviderDetails(
                Mock.Of<ILogger<GetTrainingProviderDetails>>(),
                outerApiClient);

            //sut
            Func<Task> result = async () =>
            {
                await _getTrainingProviderDetails.GetTrainingProvider(It.IsAny<long>());
            };

            //assert
            await result.Should().ThrowAsync<Exception>();
        }
    }
}
