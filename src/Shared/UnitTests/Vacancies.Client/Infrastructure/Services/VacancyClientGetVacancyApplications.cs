using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.VacancyApplications;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Infrastructure.Services
{
    public class VacancyClientGetVacancyApplications
    {
        [Test, MoqAutoData]
        public async Task Then_The_Service_Is_Called_And_VacancyApplications_Returned(
            long vacancyReference,
            List<ApplicationReview> applicationReviews,
            [Frozen] Mock<IApplicationReviewRepository> applicationReviewRepository,
            VacancyClient vacancyClient)
        {
            //Arrange
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<ApplicationReview>(vacancyReference))
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
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<ApplicationReview>(vacancyReference))
                .ReturnsAsync(new List<ApplicationReview>());
            
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
            applicationReviewRepository.Setup(x => x.GetForVacancyAsync<ApplicationReview>(vacancyReference))
                .ReturnsAsync((List<ApplicationReview>) null);
            
            //Act
            var actual = await vacancyClient.GetVacancyApplicationsAsync(vacancyReference);
            
            //Assert
            actual.Should().BeEmpty();
        }
    }
}