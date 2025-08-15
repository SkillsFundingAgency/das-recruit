using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class TrainingOrchestratorGetTests
    {
        
        [Test, MoqAutoData]
        public async Task Then_Returns_True_If_Has_Multiple_Legal_Entities(
            VacancyRouteModel vacancyRouteModel, 
            EmployerInfo employerInfo,
            Vacancy vacancy,
            VacancyUser vacancyUser,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IProviderVacancyClient> providerRecruitVacancyClient,
            TrainingOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Training_Get))
                .ReturnsAsync(vacancy);
            providerRecruitVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId))
                .ReturnsAsync(employerInfo);
            
            var actual = await orchestrator.GetTrainingViewModelAsync(vacancyRouteModel, vacancyUser);

            actual.HasMoreThanOneLegalEntity.Should().BeTrue();
        }
        
        [Test, MoqAutoData]
        public async Task Then_Returns_False_If_Has_One_Legal_Entities(
            VacancyRouteModel vacancyRouteModel, 
            EmployerInfo employerInfo,
            Vacancy vacancy,
            VacancyUser vacancyUser,
            LegalEntity legalEntity,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IProviderVacancyClient> providerRecruitVacancyClient,
            TrainingOrchestrator orchestrator)
        {
            employerInfo.LegalEntities = new List<LegalEntity>
            {
                legalEntity
            };
        
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.Training_Get))
                .ReturnsAsync(vacancy);
            providerRecruitVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId))
                .ReturnsAsync(employerInfo);
            
            var actual = await orchestrator.GetTrainingViewModelAsync(vacancyRouteModel, vacancyUser);

            actual.HasMoreThanOneLegalEntity.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Then_ReturnsFilteredProgrammes_ForGivenUkprn(
                    [Frozen] Mock<IProviderVacancyClient> mockProviderVacancyClient,
                    [Frozen] Mock<IUtility> mockUtility,
                    [Frozen] Mock<IReviewSummaryService> mockReviewSummaryService,
                    [Frozen] Mock<IRecruitVacancyClient> mockRecruitVacancyClient,
                    TrainingOrchestrator orchestrator)
        {
            // Arrange
            var ukprn = 12345678;
            var vacancyId = System.Guid.NewGuid();

            var expectedProgrammes = new List<ApprenticeshipProgramme>
            {
                new ApprenticeshipProgramme { Id = "123", Title = "Software Developer", IsActive = true },
                new ApprenticeshipProgramme { Id = "456", Title = "Data Analyst", IsActive = true }
            };

            var vacancy = new Vacancy
            {
                Id = vacancyId,
                Title = "Test Vacancy",
                ProgrammeId = "123",
                EmployerAccountId = "EMP123",
                Status = VacancyStatus.Draft
            };

            var routeModel = new VacancyRouteModel { Ukprn = ukprn, VacancyId = vacancyId };
            var user = new VacancyUser { Ukprn = ukprn };

            mockProviderVacancyClient
                .Setup(x => x.GetActiveApprenticeshipProgrammesAsync(ukprn))
                .ReturnsAsync(expectedProgrammes);

            mockUtility
                .Setup(x => x.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.Training_Get))
                .ReturnsAsync(vacancy);

            // Act
            var viewModel = await orchestrator.GetTrainingViewModelAsync(routeModel, user);

            // Assert
            viewModel.Should().NotBeNull();
            viewModel.Title.Should().Be("Test Vacancy");
            viewModel.Programmes.Should().HaveCount(2);
            viewModel.Programmes.Should().Contain(p => p.Id == "123");
            viewModel.Programmes.Should().Contain(p => p.Id == "456");

            mockProviderVacancyClient.Verify(x => x.GetActiveApprenticeshipProgrammesAsync(ukprn), Times.Once);
        }
    }
}