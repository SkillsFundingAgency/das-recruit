using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.ApplicationReview;

[TestFixture]
internal class ApplicationReviewServiceTests
{
    [Test, MoqAutoData]
    public async Task GetForVacancySortedAsync_ReturnsSortedApplicationReviews(long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        var apiResponse = new GetApplicationReviewsByVacancyReferenceApiResponse
        {
            ApplicationReviews =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    VacancyReference = vacancyReference,
                    Status = ApplicationReviewStatus.New.ToString(),
                    TemporaryReviewStatus = null,
                    CreatedDate = new DateTime(2024, 1, 1),
                    DateSharedWithEmployer = null,
                    ReviewedDate = null,
                    SubmittedDate = new DateTime(2024, 1, 2),
                    WithdrawnDate = null,
                    CandidateFeedback = "Feedback1",
                    EmployerFeedback = "EmpFeedback1",
                    VacancyTitle = "Vacancy1",
                    HasEverBeenEmployerInterviewing = false,
                    AdditionalQuestion1 = "Q1",
                    AdditionalQuestion2 = "Q2",
                    Application = new GetApplicationReviewsByVacancyReferenceApiResponse.Application
                    {
                        Id = Guid.NewGuid(),
                        CandidateId = Guid.NewGuid(),
                        Candidate = new GetApplicationReviewsByVacancyReferenceApiResponse.Candidate
                        {
                            FirstName = "John",
                            LastName = "Doe"
                        },
                        EmploymentLocation = null
                    }
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    VacancyReference = vacancyReference,
                    Status = ApplicationReviewStatus.New.ToString(),
                    TemporaryReviewStatus = null,
                    CreatedDate = new DateTime(2024, 1, 2),
                    DateSharedWithEmployer = null,
                    ReviewedDate = null,
                    SubmittedDate = new DateTime(2024, 1, 1),
                    WithdrawnDate = null,
                    CandidateFeedback = "Feedback2",
                    EmployerFeedback = "EmpFeedback2",
                    VacancyTitle = "Vacancy2",
                    HasEverBeenEmployerInterviewing = true,
                    AdditionalQuestion1 = "Q3",
                    AdditionalQuestion2 = "Q4",
                    Application = null
                }
            ]
        };

        mockApiClient
            .Setup(x => x.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                It.IsAny<GetApplicationReviewsByVacancyReferenceApiRequest>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await service.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(2);
        result[0].CandidateFeedback.Should().Be("Feedback2");
        result[1].CandidateFeedback.Should().Be("Feedback1");
    }

    [Test, MoqAutoData]
    public async Task GetForVacancySortedAsync_ReturnsEmptyList_WhenNoApplicationReviews(
        long vacancyReference,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        mockApiClient
            .Setup(x => x.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                It.IsAny<GetApplicationReviewsByVacancyReferenceApiRequest>()))
            .ReturnsAsync(new GetApplicationReviewsByVacancyReferenceApiResponse
            {
                ApplicationReviews = []
            });

        // Act
        var result = await service.GetForVacancySortedAsync(vacancyReference, SortColumn.Name, SortOrder.Ascending);

        // Assert
        result.Should().BeNullOrEmpty();
    }

    [Test, MoqAutoData]
    public async Task GetForSharedVacancySortedAsync_ReturnsSortedApplicationReviews(long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        var apiResponse = new GetApplicationReviewsByVacancyReferenceApiResponse
        {
            ApplicationReviews =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    VacancyReference = vacancyReference,
                    Status = ApplicationReviewStatus.New.ToString(),
                    TemporaryReviewStatus = null,
                    CreatedDate = new DateTime(2024, 1, 1),
                    DateSharedWithEmployer = null,
                    ReviewedDate = null,
                    SubmittedDate = new DateTime(2024, 1, 2),
                    WithdrawnDate = null,
                    CandidateFeedback = "Feedback1",
                    EmployerFeedback = "EmpFeedback1",
                    VacancyTitle = "Vacancy1",
                    HasEverBeenEmployerInterviewing = false,
                    AdditionalQuestion1 = "Q1",
                    AdditionalQuestion2 = "Q2",
                    Application = new GetApplicationReviewsByVacancyReferenceApiResponse.Application
                    {
                        Id = Guid.NewGuid(),
                        CandidateId = Guid.NewGuid(),
                        Candidate = new GetApplicationReviewsByVacancyReferenceApiResponse.Candidate
                        {
                            FirstName = "John",
                            LastName = "Doe"
                        },
                        EmploymentLocation = null
                    }
                },

                new()
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    VacancyReference = vacancyReference,
                    Status = ApplicationReviewStatus.New.ToString(),
                    TemporaryReviewStatus = null,
                    CreatedDate = new DateTime(2024, 1, 2),
                    DateSharedWithEmployer = new DateTime(2024, 1, 1),
                    ReviewedDate = null,
                    SubmittedDate = new DateTime(2024, 1, 1),
                    WithdrawnDate = null,
                    CandidateFeedback = "Feedback2",
                    EmployerFeedback = "EmpFeedback2",
                    VacancyTitle = "Vacancy2",
                    HasEverBeenEmployerInterviewing = true,
                    AdditionalQuestion1 = "Q3",
                    AdditionalQuestion2 = "Q4",
                    Application = null
                }
            ]
        };

        mockApiClient
            .Setup(x => x.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                It.IsAny<GetApplicationReviewsByVacancyReferenceApiRequest>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await service.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(1);
        result[0].CandidateFeedback.Should().Be("Feedback2");
    }

    [Test, MoqAutoData]
    public async Task GetForSharedVacancySortedAsync_ReturnsEmptyList_WhenNoApplicationReviews(
        long vacancyReference,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        mockApiClient
            .Setup(x => x.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                It.IsAny<GetApplicationReviewsByVacancyReferenceApiRequest>()))
            .ReturnsAsync(new GetApplicationReviewsByVacancyReferenceApiResponse
            {
                ApplicationReviews = []
            });

        // Act
        var result = await service.GetForSharedVacancySortedAsync(vacancyReference, SortColumn.Name, SortOrder.Ascending);

        // Assert
        result.Should().BeNullOrEmpty();
    }
}
