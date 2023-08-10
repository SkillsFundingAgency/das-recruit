using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;
using GetTrainingProvidersResponse = Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.GetTrainingProvidersResponse;

namespace Esfa.Recruit.Employer.UnitTests.Recruit.Vacancies.Client.Infrastructure.ReferenceData
{
    public class TrainingProvidersUpdateServiceTest
    {
        private readonly Mock<IOptions<OuterApiConfiguration>> _mockOptions;
        private readonly Mock<IReferenceDataWriter> _referenceDataWriter;
        private TrainingProvidersUpdateService _getTrainingProviderDetails;

        public TrainingProvidersUpdateServiceTest()
        {
            _mockOptions = new Mock<IOptions<OuterApiConfiguration>>();
            _referenceDataWriter = new Mock<IReferenceDataWriter>();
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
                Content = new StringContent(JsonConvert.SerializeObject(new GetTrainingProvidersResponse()), Encoding.UTF8, "application/json"),
                StatusCode = HttpStatusCode.InternalServerError
            };
            var httpMessageHandler = MessageHandler.SetupMessageHandlerMock(response, appSettings.BaseUrl);
            var client = new HttpClient(httpMessageHandler.Object);
            var outerApiClient = new OuterApiClient(client, _mockOptions.Object);

            _getTrainingProviderDetails = new TrainingProvidersUpdateService(
                Mock.Of<ILogger<TrainingProvidersUpdateService>>(),
                _referenceDataWriter.Object,
                outerApiClient);

            //sut
            Func<Task> result = async () =>
            {
                await _getTrainingProviderDetails.UpdateProviders();
            };

            //assert
            await result.Should().ThrowAsync<Exception>();
        }
    }
}
