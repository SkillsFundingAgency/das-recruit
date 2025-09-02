using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.UnitTests.TestHelpers;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using NUnit.Framework;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

public class WhenGettingAllApprenticeshipProgrammes
{
    [Test, MoqAutoData]
    public async Task Then_The_Courses_Are_Retrieved_From_The_Api(
        GetTrainingProgrammesResponse apiResponse,
        [Frozen] Mock<IConfiguration> mockConfiguration,
        [Frozen] Mock<ITimeProvider> mockTimeProvider,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        outerApiClient
            .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
            .ReturnsAsync(apiResponse);
        mockConfiguration.Setup(x=>x["ResourceEnvironmentName"]).Returns("LOCAL");
        var cache = new TestCache();
        var provider = new ApprenticeshipProgrammeProvider(cache, mockTimeProvider.Object, outerApiClient.Object, Mock.Of<IFeature>(), mockConfiguration.Object);
        
        var actual = await provider.GetApprenticeshipProgrammesAsync(true);

        actual.Should().BeEquivalentTo(apiResponse.TrainingProgrammes.Select(c => (ApprenticeshipProgramme)c).ToList());
    }
}