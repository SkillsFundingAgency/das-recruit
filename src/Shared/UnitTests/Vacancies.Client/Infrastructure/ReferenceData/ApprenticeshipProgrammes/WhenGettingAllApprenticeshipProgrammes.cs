using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
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
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        // Arrange
        outerApiClient
            .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
            .ReturnsAsync(apiResponse);

        var provider = new ApprenticeshipProgrammeProvider(outerApiClient.Object);

        var expected = apiResponse
            .TrainingProgrammes
            .Select(c => (ApprenticeshipProgramme)c)
            .ToList();

        expected.Add(GetDummyProgramme());

        // Act
        var actual = await provider.GetApprenticeshipProgrammesAsync(true);

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
    public async Task Then_If_The_ProgrammeId_Is_DummyCourses_And_Retrieved_As_Expected(
        GetTrainingProgrammesResponse apiResponse,
        [Frozen] Mock<IOuterApiClient> outerApiClient)
    {
        outerApiClient
            .Setup(x => x.Get<GetTrainingProgrammesResponse>(It.IsAny<GetTrainingProgrammesRequest>()))
            .ReturnsAsync(apiResponse);

        var provider = new ApprenticeshipProgrammeProvider(outerApiClient.Object);

        var actual = await provider.GetApprenticeshipProgrammeAsync(EsfaDummyTrainingProgramme.Id.ToString());

        actual.Id.Should().Be(EsfaDummyTrainingProgramme.Id.ToString());
        actual.Title.Should().Be(EsfaDummyTrainingProgramme.Title);
        actual.ApprenticeshipType.Should().Be(EsfaDummyTrainingProgramme.ApprenticeshipType);
        actual.ApprenticeshipLevel.Should().Be(EsfaDummyTrainingProgramme.ApprenticeshipLevel);
        actual.LastDateStarts.Should().BeAfter(DateTime.UtcNow);
        actual.EffectiveTo.Should().BeAfter(DateTime.UtcNow);
    }

    private static ApprenticeshipProgramme GetDummyProgramme() =>
        new()
        {
            Id = EsfaDummyTrainingProgramme.Id.ToString(),
            Title = EsfaDummyTrainingProgramme.Title,
            IsActive = true,
            ApprenticeshipType = EsfaDummyTrainingProgramme.ApprenticeshipType,
            ApprenticeshipLevel = EsfaDummyTrainingProgramme.ApprenticeshipLevel,
            EffectiveTo = DateTime.UtcNow.AddYears(1),
            LastDateStarts = DateTime.UtcNow.AddYears(1)
        };
}