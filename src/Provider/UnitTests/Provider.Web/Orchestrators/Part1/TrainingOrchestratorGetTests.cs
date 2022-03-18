using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
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
    }
}