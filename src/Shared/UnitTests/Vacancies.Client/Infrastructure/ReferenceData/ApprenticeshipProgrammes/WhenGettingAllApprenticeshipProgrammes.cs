using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.UnitTests.TestHelpers;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;

public class WhenGettingAllApprenticeshipProgrammes
{
    [Test, MoqAutoData]
    public async Task Then_The_Courses_Are_Retrieved_From_The_Api(
        GetTrainingProgrammesResponse apiResponse,
        [Frozen] Mock<ITimeProvider> mockTimeProvider,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        // Arrange
        var cache = new TestCache();
        outerApiClient
            .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
            .ReturnsAsync(apiResponse);

        var provider = new ApprenticeshipProgrammeProvider(outerApiClient.Object, cache, mockTimeProvider.Object);

        var expected = apiResponse
            .TrainingProgrammes
            .Select(c => (ApprenticeshipProgramme)c)
            .ToList();

        expected.Add(GetDummyProgramme());

        // Act
        var actual = await provider.GetApprenticeshipProgrammesAsync(true, null, includePlaceholderProgramme: true);

        // Assert
        actual.Should().BeEquivalentTo(expected, options =>
            options
                // Ignore time part for DateTime
                .Using<DateTime>(ctx =>
                    ctx.Subject.Date.Should().Be(ctx.Expectation.Date))
                .WhenTypeIs<DateTime>()

                // Also ignore time for nullable DateTime?
                .Using<DateTime?>(ctx =>
                    ctx.Subject?.Date.Should().Be(ctx.Expectation?.Date))
                .WhenTypeIs<DateTime?>()
        );
    }

    [Test, MoqAutoData]
    public async Task Then_If_The_Courses_Are_Cached_Api_Not_Called_And_Retrieved_From_The_Cached(
        Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes.ApprenticeshipProgrammes response,
        [Frozen] Mock<ICache> cache,
        [Frozen] Mock<ITimeProvider> mockTimeProvider,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        var dateTime = new DateTime(2025, 2, 1, 6, 0, 0);
        mockTimeProvider.Setup(x => x.NextDay6am).Returns(dateTime);
        cache
            .Setup(x => x.CacheAsideAsync(CacheKeys.ApprenticeshipProgrammes, dateTime, It.IsAny<Func<Task<Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes.ApprenticeshipProgrammes>>>()))
            .ReturnsAsync(response);
        var provider = new ApprenticeshipProgrammeProvider(outerApiClient.Object, cache.Object, mockTimeProvider.Object);

        var actual = await provider.GetApprenticeshipProgrammesAsync(true);

        actual.Should().BeEquivalentTo(response.Data);
        outerApiClient
            .Verify(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()), Times.Never);
    }

    [Test, MoqAutoData]
    public async Task Then_If_The_ProgrammeId_Is_DummyCourses_And_Retrieved_As_Expected(
        GetTrainingProgrammesResponse apiResponse,
        [Frozen] Mock<ITimeProvider> mockTimeProvider,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        var cache = new TestCache();
        outerApiClient
            .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
            .ReturnsAsync(apiResponse);

        var provider = new ApprenticeshipProgrammeProvider(outerApiClient.Object, cache, mockTimeProvider.Object);

        var actual = await provider.GetApprenticeshipProgrammeAsync(EsfaTestTrainingProgramme.Id.ToString(), includePlaceholderProgramme: true);

        actual.Id.Should().Be(EsfaTestTrainingProgramme.Id.ToString());
        actual.Title.Should().Be(EsfaTestTrainingProgramme.Title);
        actual.ApprenticeshipType.Should().Be(EsfaTestTrainingProgramme.ApprenticeshipType);
        actual.ApprenticeshipLevel.Should().Be(EsfaTestTrainingProgramme.ApprenticeshipLevel);
        actual.LastDateStarts.Should().BeAfter(DateTime.UtcNow);
        actual.EffectiveTo.Should().BeAfter(DateTime.UtcNow);
    }

    private static ApprenticeshipProgramme GetDummyProgramme() =>
        new()
        {
            Id = EsfaTestTrainingProgramme.Id.ToString(),
            Title = EsfaTestTrainingProgramme.Title,
            IsActive = true,
            ApprenticeshipType = EsfaTestTrainingProgramme.ApprenticeshipType,
            ApprenticeshipLevel = EsfaTestTrainingProgramme.ApprenticeshipLevel,
            EffectiveTo = DateTime.UtcNow.AddYears(1),
            LastDateStarts = DateTime.UtcNow.AddYears(1)
        };
}