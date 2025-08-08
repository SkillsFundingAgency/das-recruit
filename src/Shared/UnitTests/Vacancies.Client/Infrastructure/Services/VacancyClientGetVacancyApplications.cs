using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyClientGetVacancyApplications
    {
        [Test, MoqAutoData]
        public async Task Then_The_Service_Is_Called_And_VacancyApplications_Returned(
            long vacancyReference,
            List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview> applicationReviews,
            [Frozen] Mock<ISqlDbRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>(vacancyReference))
                .ReturnsAsync(applicationReviews);
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEquivalentTo(applicationReviews.Select(c=>(VacancyApplication)c).ToList());
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Empty_List_If_No_Results(
            long vacancyReference,
            [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>(vacancyReference))
                .ReturnsAsync(new List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>());
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEmpty();
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_Empty_List_If_Null(
            long vacancyReference,
            [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>(vacancyReference))
                .ReturnsAsync((List<Recruit.Vacancies.Client.Domain.Entities.ApplicationReview>) null);
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEmpty();
        }

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