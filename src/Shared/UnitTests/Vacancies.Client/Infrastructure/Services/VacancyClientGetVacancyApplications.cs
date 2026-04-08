using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyClientGetVacancyApplications
    {
        [Test, MoqAutoData]
        public async Task GetVacancyApplicationsSortedAsync_ReturnsEmptyList_WhenNoApplications(
            long vacancyReference,
            [Frozen] Mock<ISqlDbRepository> mockAppReviewRepo,
            [Greedy] VacancyClient vacancyClient)
        {
            // Arrange
            mockAppReviewRepo.Setup(r => r.GetForVacancySortedAsync(vacancyReference, It.IsAny<SortColumn>(), It.IsAny<SortOrder>()))
                .ReturnsAsync([]);

            // Act
            var result = await vacancyClient.GetVacancyApplicationsSortedAsync(vacancyReference, SortColumn.DateApplied, SortOrder.Ascending, false);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }
    }
}