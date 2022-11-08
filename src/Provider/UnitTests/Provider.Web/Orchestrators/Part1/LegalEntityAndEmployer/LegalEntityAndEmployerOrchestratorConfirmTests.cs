using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1.LegalEntityAndEmployer
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
            vacancyRouteModel.VacancyId = null;
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
        public async Task Then_The_ConfirmLegalEntityViewModel_Is_Returned_From_The_VacancyId_If_Present(
            Vacancy vacancy,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            [Frozen] Mock<IUtility> utility,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            utility.Setup(x =>
                    x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);
            
            employerInfo.EmployerAccountId = vacancy.EmployerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, vacancy.EmployerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);
            
            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, null, null);

            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
        }

        [Test, MoqAutoData]
        public async Task Then_The_ConfirmLegalEntityViewModel_Is_Returned_From_The_VacancyId_If_Present_But_New_Selected_Values_Used(
            string employerAccountId,
            string accountLegalEntityPublicHashedId,
            Vacancy vacancy,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            [Frozen] Mock<IUtility> utility,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            utility.Setup(x =>
                    x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);
            
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, accountLegalEntityPublicHashedId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);
            
            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, accountLegalEntityPublicHashedId);

            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
            actual.VacancyId.Should().Be(vacancy.Id);
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
            vacancyRouteModel.VacancyId = null;
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
            vacancyRouteModel.VacancyId = null;
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
            vacancyRouteModel.VacancyId = null;
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = employerAccountLegalEntityId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(false);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.Recruitment))
                .ReturnsAsync(false);
            
            Assert.ThrowsAsync<MissingPermissionsException>(() => orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId));
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_In_ProviderPermissions_Recruitment_Then_Returns_Model(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            vacancyRouteModel.VacancyId = null;
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = employerAccountLegalEntityId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(false);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.Recruitment))
                .ReturnsAsync(true);
            
            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId);
            
            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_In_ProviderPermissionsRecruitmentRequiresReview_Then_Returns_Model(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            vacancyRouteModel.VacancyId = null;
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = employerAccountLegalEntityId;
            
            providerVacancyClient
                .Setup(x => x.GetProviderEmployerVacancyDataAsync(vacancyRouteModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.RecruitmentRequiresReview))
                .ReturnsAsync(true);
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(vacancyRouteModel.Ukprn,
                    employerAccountId, employerAccountLegalEntityId, OperationType.Recruitment))
                .ReturnsAsync(false);
            
            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId,
                employerAccountLegalEntityId);
            
            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
        }
    }
}