using System.Collections.Generic;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
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
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync(expected);
        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);

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
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync(expected);
        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);

        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, true);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesMongoDbRepository_WhenMongoMigrationDisabled_AndNotShared(
        long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IMongoDbRepository> mongoDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        mongoDbRepositoryMock.Setup(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync(expected);

        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);

        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, false);

        result.Should().ContainSingle();
        mongoDbRepositoryMock.Verify(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesMongoDbRepository_ForShared_WhenMongoMigrationDisabled(long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IMongoDbRepository> mongoDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        mongoDbRepositoryMock.Setup(r => r.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync(expected);

        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);

        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, true);

        result.Should().ContainSingle();
        mongoDbRepositoryMock.Verify(r => r.GetForSharedVacancySortedAsync(vacancyReference, sortColumn, sortOrder), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_ReturnsEmptyList_WhenNullReturned(long vacancyReference,
        SortColumn sortColumn,
        SortOrder sortOrder,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        sqlDbRepositoryMock.Setup(r => r.GetForVacancySortedAsync(vacancyReference, sortColumn, sortOrder))
            .ReturnsAsync((List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>)null);

        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);

        var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, sortColumn, sortOrder, false);

        result.Should().BeEmpty();
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesSqlDbRepository_WhenMongoMigrationEnabled_AndNotShared(
        long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync(expected);
        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);

        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForSharedVacancyAsync(vacancyReference), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsSortedAsync_UsesSqlDbRepository_ForShared_WhenMongoMigrationEnabled(
        long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync(expected);
        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);

        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().ContainSingle();
        sqlDbRepositoryMock.Verify(r => r.GetForSharedVacancyAsync(vacancyReference), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsAsync_UsesMongoDbRepository_WhenMongoMigrationDisabled_AndNotShared(
        long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IMongoDbRepository> mongoDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        mongoDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference    ))
            .ReturnsAsync(expected);

        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);

        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().ContainSingle();
        mongoDbRepositoryMock.Verify(r => r.GetForSharedVacancyAsync(vacancyReference), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsAsync_UsesMongoDbRepository_ForShared_WhenMongoMigrationDisabled(long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<IMongoDbRepository> mongoDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        var expected = new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> { applicationReview };
        mongoDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync(expected);

        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(false);

        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().ContainSingle();
        mongoDbRepositoryMock.Verify(r => r.GetForSharedVacancyAsync(vacancyReference), Times.Once);
    }

    [Test, MoqAutoData]
    public async Task GetVacancyApplicationsAsync_ReturnsEmptyList_WhenNullReturned(long vacancyReference,
        Recruit.Vacancies.Client.Domain.Entities.ApplicationReview applicationReview,
        [Frozen] Mock<IFeature> feature,
        [Frozen] Mock<ISqlDbRepository> sqlDbRepositoryMock,
        [Greedy] VacancyClient vacancyClient)
    {
        sqlDbRepositoryMock.Setup(r => r.GetForSharedVacancyAsync(vacancyReference))
            .ReturnsAsync((List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>)null);

        feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigration)).Returns(true);

        var result = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference, true);

        result.Should().BeEmpty();
    }
}