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
                    Application = new Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses.Application
                    {
                        Id = Guid.NewGuid(),
                        CandidateId = Guid.NewGuid(),
                        Candidate = new Candidate
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
                    Application = new Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses.Application
                    {
                        Id = Guid.NewGuid(),
                        CandidateId = Guid.NewGuid(),
                        Candidate = new Candidate
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

    [Test, MoqAutoData]
    public async Task GetAsync_ReturnsMappedApplicationReview_WhenApiReturnsResponse(Guid applicationReviewId,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        var apiResponse = new GetApplicationReviewByIdApiResponse
        {
            ApplicationReview = new Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses.ApplicationReview
            {
                Id = applicationReviewId,
                CandidateId = Guid.NewGuid(),
                VacancyReference = 1234,
                Status = ApplicationReviewStatus.InReview.ToString(),
                TemporaryReviewStatus = null,
                CreatedDate = DateTime.UtcNow,
                DateSharedWithEmployer = DateTime.UtcNow,
                ReviewedDate = DateTime.UtcNow,
                SubmittedDate = DateTime.UtcNow,
                WithdrawnDate = null,
                CandidateFeedback = "feedback",
                EmployerFeedback = "employer feedback",
                VacancyTitle = "Vacancy",
                HasEverBeenEmployerInterviewing = false,
                AdditionalQuestion1 = "Q1",
                AdditionalQuestion2 = "Q2",
                Application = new Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses.Application
                {
                    Id = Guid.NewGuid(),
                    CandidateId = Guid.NewGuid(),
                    Candidate = new Candidate
                    {
                        FirstName = "John",
                        LastName = "Doe"
                    },
                    EmploymentLocation = new Location
                    {
                        Addresses = []
                    }
                }
            }
        };

        mockApiClient.Setup(x => x.Get<GetApplicationReviewByIdApiResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await service.GetAsync(applicationReviewId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(applicationReviewId);
        result.CandidateId.Should().Be(apiResponse.ApplicationReview.CandidateId);
        result.Status.Should().Be(ApplicationReviewStatus.InReview);
        result.CandidateFeedback.Should().Be(apiResponse.ApplicationReview.CandidateFeedback);
        result.EmployerFeedback.Should().Be(apiResponse.ApplicationReview.EmployerFeedback);
        result.VacancyTitle.Should().Be(apiResponse.ApplicationReview.VacancyTitle);
        result.HasEverBeenEmployerInterviewing.Should().Be(apiResponse.ApplicationReview.HasEverBeenEmployerInterviewing);
        result.AdditionalQuestion1.Should().Be(apiResponse.ApplicationReview.AdditionalQuestion1);
        result.AdditionalQuestion2.Should().Be(apiResponse.ApplicationReview.AdditionalQuestion2);
        result.Application.Should().NotBeNull();
        result.Application.ApplicationId.Should().Be(apiResponse.ApplicationReview.Application.Id);
        result.Application.CandidateId.Should().Be(apiResponse.ApplicationReview.Application.CandidateId);
        result.Application.FirstName.Should().Be(apiResponse.ApplicationReview.Application.Candidate.FirstName);
        result.Application.LastName.Should().Be(apiResponse.ApplicationReview.Application.Candidate.LastName);
    }

    [Test, MoqAutoData]
    public async Task GetAsync_ReturnsNull_WhenApiReturnsNull(Guid applicationReviewId,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        var apiResponse = new GetApplicationReviewByIdApiResponse
        {
            ApplicationReview = null
        };

        mockApiClient.Setup(x => x.Get<GetApplicationReviewByIdApiResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await service.GetAsync(applicationReviewId);

        // Assert
        result.Should().BeNull();
    }

    [Test, MoqAutoData]
    public async Task GetAsync_WithdrawnDate_Not_Null_ReturnsNull(Guid applicationReviewId,
        [Frozen] Mock<IOuterApiClient> mockApiClient,
        [Greedy] ApplicationReviewService service)
    {
        // Arrange
        var apiResponse = new GetApplicationReviewByIdApiResponse
        {
            ApplicationReview = new Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses.ApplicationReview()
            {
                WithdrawnDate = DateTime.Now
            }
        };

        mockApiClient.Setup(x => x.Get<GetApplicationReviewByIdApiResponse>(It.IsAny<IGetApiRequest>()))
            .ReturnsAsync(apiResponse);

        // Act
        var result = await service.GetAsync(applicationReviewId);

        // Assert
        result.Should().BeNull();
    }
}
