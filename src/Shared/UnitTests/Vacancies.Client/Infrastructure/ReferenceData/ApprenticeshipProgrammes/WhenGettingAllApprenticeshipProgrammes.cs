using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
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

        var actual = await provider.GetApprenticeshipProgrammeAsync("999999");

        actual.Id.Should().Be("999999");
        actual.Title.Should().Be("To be confirmed");
        actual.ApprenticeshipType.Should().Be(TrainingType.Standard);
        actual.ApprenticeshipLevel.Should().Be(ApprenticeshipLevel.Unknown);
        actual.LastDateStarts.Should().BeAfter(DateTime.UtcNow);
        actual.EffectiveTo.Should().BeAfter(DateTime.UtcNow);
    }

    private static ApprenticeshipProgramme GetDummyProgramme() =>
        new()
        {
            Id = "999999",
            Title = "To be confirmed",
            IsActive = true,
            ApprenticeshipType = TrainingType.Standard,
            ApprenticeshipLevel = ApprenticeshipLevel.Unknown,
            EffectiveTo = DateTime.UtcNow.AddYears(1),
            LastDateStarts = DateTime.UtcNow.AddYears(1)
        };
}