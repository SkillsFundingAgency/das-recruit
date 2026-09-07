using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Client;

[TestFixture]
public class VacancyClientTests
{
    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesSqlDbRepository_WhenMongoMigrationEnabled_AndNotShared(
        long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IApplicationReadRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync(expected);

        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, false);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesSqlDbRepository_ForShared_WhenMongoMigrationEnabled(
        long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IApplicationReadRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync(expected);

        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, true);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder), Times.Once);
    }

    
    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_ReturnsEmptyList_WhenNullReturned(long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IApplicationReadRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        sqlDbRepositoryMock.Setup(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync((List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>)null);
        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, false);

        result.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task GetApplicationReviewAsync_UsesSqlDbRepository_WhenMongoMigrationEnabled(
        Guid applicationReviewId,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IApplicationReadRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        applicationReview.Application.ApplicationId = applicationReviewId;
        sqlDbRepositoryMock.Setup(r => r.GetAsync(applicationReviewId))
            .ReturnsAsync(applicationReview);
        var result = await vacancyClient.GetApplicationReviewAsync(applicationReviewId);

        result.Application.ApplicationId.Should().Be(applicationReviewId);
        sqlDbRepositoryMock.Verify(r => r.GetAsync(applicationReviewId), Times.Once);
    }

    [Test]
    [MoqInlineAutoData(ApplicationReviewStatus.Successful, true)]
    [MoqInlineAutoData(ApplicationReviewStatus.Unsuccessful, true)]
    [MoqInlineAutoData(ApplicationReviewStatus.AllShared, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.InReview, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.Interviewing, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.EmployerInterviewing, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.EmployerUnsuccessful, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.PendingShared, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.PendingToMakeUnsuccessful, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.New, false)]
    [MoqInlineAutoData(ApplicationReviewStatus.Shared, false)]
    public async Task IsAllApplicationReviewsHasOutcomeAsync_Returns_Valid(
        ApplicationReviewStatus status,
        bool expectedResult,
        List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> applicationReviews,
        [Frozen] Mock<IApplicationReadRepository> sqlDbRepositoryMock,
        [Frozen] Mock<IVacancyRepository> vacancyRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var vacancyId = Guid.NewGuid();
        applicationReviews.ForEach(ar =>
        {
            ar.IsWithdrawn = false;
            ar.Status = status;
        });

        vacancyRepositoryMock.Setup(x => x.GetVacancyAsync(vacancyId)).ReturnsAsync(new Vacancy { Id = vacancyId, Status = VacancyStatus.Closed });

        sqlDbRepositoryMock.Setup(x => x.GetForVacancyAsync<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>(vacancyId))
            .ReturnsAsync(applicationReviews);

        var result = await vacancyClient.IsAllApplicationReviewsHasOutcomeAsync(vacancyId);

        result.Should().Be(expectedResult);
    }

    [Test, MoqAutoData]
    public async Task When_Vacancy_Not_Closed_IsAllApplicationReviewsHasOutcomeAsync_Returns_Invalid(
        ApplicationReviewStatus status,
        List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> applicationReviews,
        [Frozen] Mock<IApplicationReadRepository> sqlDbRepositoryMock,
        [Frozen] Mock<IVacancyRepository> vacancyRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var vacancyId = Guid.NewGuid();
        applicationReviews.ForEach(ar =>
        {
            ar.IsWithdrawn = false;
            ar.Status = status;
        });

        vacancyRepositoryMock.Setup(x => x.GetVacancyAsync(vacancyId)).ReturnsAsync(new Vacancy { Id = vacancyId, Status = VacancyStatus.Live });

        sqlDbRepositoryMock.Setup(x => x.GetForVacancyAsync<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>(vacancyId))
            .ReturnsAsync(applicationReviews);

        var result = await vacancyClient.IsAllApplicationReviewsHasOutcomeAsync(vacancyId);

        result.Should().Be(false);
    }
}