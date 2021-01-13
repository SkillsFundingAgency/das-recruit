using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders
{
    public class TrainingProviderUpdateServiceTests
    {
        private Mock<IOuterApiClient> _mockOuterApiClient;
        private Mock<IReferenceDataWriter> _mockReferenceDataWriter;
        private TrainingProvidersUpdateService _sut;

        public TrainingProviderUpdateServiceTests ()
        {
            var mockReferenceDataReader = new Mock<IReferenceDataReader>();
            _mockOuterApiClient = new Mock<IOuterApiClient>();
            _mockReferenceDataWriter = new Mock<IReferenceDataWriter>();

            _sut = new TrainingProvidersUpdateService(Mock.Of<ILogger<TrainingProvidersUpdateService>>(),
                _mockReferenceDataWriter.Object,
                mockReferenceDataReader.Object,
                _mockOuterApiClient.Object);
        }

        [Fact]
        public async Task Then_The_Data_Is_Taken_From_The_Outer_Api_And_Upserted()
        {
            // Arrange
            var fixture = new Fixture();
            var providerResponse = fixture.Create<GetProvidersResponse>();
            _mockOuterApiClient
                .Setup(x => x.Get<GetProvidersResponse>(It.IsAny<GetProvidersRequest>()))
                .ReturnsAsync(providerResponse);
            
            Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders trainingProviders = null;

            _mockReferenceDataWriter.Setup(x => x.UpsertReferenceData(It.IsAny<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>()))
                .Callback<Recruit.Vacancies.Client.Infrastructure.ReferenceData.TrainingProviders.TrainingProviders>(arg => trainingProviders = arg)
                .Returns(Task.CompletedTask);
            
            //Act
            await _sut.UpdateProviders();
            
            //Assert
            _mockReferenceDataWriter.Verify(x => x.UpsertReferenceData(trainingProviders), Times.Once);
            trainingProviders.Data.Should()
                .BeEquivalentTo(providerResponse.Providers.Select(c => (TrainingProvider) c).ToList());
        }
    }
}