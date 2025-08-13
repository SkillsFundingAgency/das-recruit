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
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
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
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
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
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        sqlDbRepositoryMock.Setup(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync((List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>)null);
        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, false);

        result.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesSqlDbRepository_WhenMongoMigrationEnabled_AndNotShared(
        long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync(expected);
        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForSharedVacancyAsync(vacancyReference), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesSqlDbRepository_ForShared_WhenMongoMigrationEnabled(
        long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync(expected);
        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForSharedVacancyAsync(vacancyReference), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsAsync_ReturnsEmptyList_WhenNullReturned(long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync((List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>)null);
        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task GetApplicationReviewAsync_UsesSqlDbRepository_WhenMongoMigrationEnabled(
        Guid applicationReviewId,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        applicationReview.Application.ApplicationId = applicationReviewId;
        sqlDbRepositoryMock.Setup(r => r.GetAsync(applicationReviewId))
            .ReturnsAsync(applicationReview);
        var result = await vacancyClient.GetApplicationReviewAsync(applicationReviewId);

        result.Application.ApplicationId.Should().Be(applicationReviewId);
        sqlDbRepositoryMock.Verify(r => r.GetAsync(applicationReviewId), Times.Once);
    }
}