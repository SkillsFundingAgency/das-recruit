using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1
{
    public class EmployerOrchestratorTests
    {
        [Test, MoqAutoData]
        public void Then_If_There_Are_No_Employers_Returned_Exception_Thrown(
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            EmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync((ProviderEditVacancyInfo) null);

            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetEmployersViewModelAsync(vacancyRouteModel));
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Employers_Then_The_ViewModel_Is_Returned_For_Apprenticeships(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            EmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetEmployersViewModelAsync(vacancyRouteModel);

            actual.Employers.Should().BeEquivalentTo(providerEditVacancyInfo.Employers.Select(e => new EmployerViewModel
                {Id = e.EmployerAccountId, Name = e.Name}));
            actual.VacancyId.Should().Be(vacancyRouteModel.VacancyId);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }

        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Employers_And_Employer_Has_Given_Recruit_Permission_Then_Returned_For_Traineeship(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipService)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);
            providerRelationshipService
                .Setup(x => x.GetLegalEntitiesForProviderAsync(vacancyRouteModel.Ukprn, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(new List<EmployerInfo>{new EmployerInfo
                {
                    EmployerAccountId = providerEditVacancyInfo.Employers.Last().EmployerAccountId,
                    LegalEntities = new List<LegalEntity>()
                }});
            
            var orchestrator = new EmployerOrchestrator(providerVacancyClient.Object,
                providerRelationshipService.Object, new ServiceParameters("Traineeship"));   
            
            var actual = await orchestrator.GetEmployersViewModelAsync(vacancyRouteModel);

            actual.Employers.Count().Should().Be(providerEditVacancyInfo.Employers.Count()-1);
            actual.Employers.First().Id.Should().Be(providerEditVacancyInfo.Employers.First().EmployerAccountId);
            actual.VacancyId.Should().Be(vacancyRouteModel.VacancyId);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Employers_And_No_Employer_Has_Given_Recruit_Permission_For_Traineeship_Then_Exception_Thrown(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipService)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);
            providerRelationshipService
                .Setup(x => x.GetLegalEntitiesForProviderAsync(vacancyRouteModel.Ukprn, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(providerEditVacancyInfo.Employers.Select(c=>new EmployerInfo
                {
                    EmployerAccountId = c.EmployerAccountId
                }));

            var orchestrator = new EmployerOrchestrator(providerVacancyClient.Object,
                providerRelationshipService.Object, new ServiceParameters("Traineeship"));
            
            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetEmployersViewModelAsync(vacancyRouteModel));
        }
    }
}