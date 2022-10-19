using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
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
    public class LegalEntityAndEmployerOrchestratorConfirmTests
    {
        
        [Test, MoqAutoData]
        public async Task Then_The_ConfirmLegalEntityViewModel_Is_Returned_From_The_AccountId_And_AccountLegalEntityId(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = employerAccountLegalEntityId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);
            
            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId);

            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
        }

        [Test, MoqAutoData]
        public void Then_If_No_Result_Found_Then_Missing_Permissions_Error_Returned(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(new EmployerInfo());
            
            Assert.ThrowsAsync<MissingPermissionsException>(() => orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId));
        }
        
        [Test, MoqAutoData]
        public void Then_If_No_Matching_Result_Found_Then_Missing_Permissions_Error_Returned(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            
            Assert.ThrowsAsync<MissingPermissionsException>(() => orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId));
        }

        [Test, MoqAutoData]
        public void Then_If_Not_In_ProviderPermissions_Then_Missing_Permissions_Error_Returned(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = employerAccountLegalEntityId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(false);
            
            Assert.ThrowsAsync<MissingPermissionsException>(() => orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId));
        }
    }
}