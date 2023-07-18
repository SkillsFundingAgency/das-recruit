using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Recruit.Vacancies.Client.Services
{
    public class TrainingProviderServiceTest
    {
        private readonly Mock<IGetTrainingProviderDetails> _mockGetTrainingProviderDetails;
        private readonly TrainingProviderService _trainingProviderService;

        public TrainingProviderServiceTest()
        {
            _mockGetTrainingProviderDetails = new Mock<IGetTrainingProviderDetails>();
            _trainingProviderService = new TrainingProviderService(
                Mock.Of<ILogger<TrainingProviderService>>(),
                new Mock<IReferenceDataReader>().Object,
                new Mock<ICache>().Object,
                new Mock<ITimeProvider>().Object,
                _mockGetTrainingProviderDetails.Object);
        }


        [Theory]
        [InlineData(ProviderTypeIdentifier.MainProvider, true)]
        [InlineData(ProviderTypeIdentifier.EmployerProvider, true)]
        [InlineData(ProviderTypeIdentifier.SupportingProvider, false)]
        public async Task Then_IsProviderMainOrEmployerProfile(ProviderTypeIdentifier typeIdentifier, bool expectedResult)
        {
            //arrange 
            _mockGetTrainingProviderDetails.Setup(x => x.GetTrainingProvider(It.IsAny<long>())).ReturnsAsync(new GetProviderResponse
            {
                ProviderType = new ProviderTypeResponse
                {
                    Id = (short)typeIdentifier
                }
            });

            //sut
            bool result = await _trainingProviderService.IsProviderMainOrEmployerProfile(It.IsAny<long>());

            //assert
            result.Should().Be(expectedResult);
        }
    }
}
