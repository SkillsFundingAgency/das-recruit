using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1.LegalEntityAndEmployer
{
    public class LegalEntityAndEmployerOrchestratorConfirmTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_ConfirmLegalEntityViewModel_Is_Returned_For_New_Vacancy(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            vacancyRouteModel.VacancyId = null;
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = employerAccountLegalEntityId;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, employerAccountLegalEntityId);

            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
        }

        [Test, MoqAutoData]
        public async Task Then_The_ConfirmLegalEntityViewModel_Is_Returned_For_Existing_Vacancy_Using_Vacancy_Employer_Details(
            Vacancy vacancy,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);

            employerInfo.EmployerAccountId = vacancy.EmployerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, null, null);

            actual.EmployerName.Should().Be(employerInfo.Name);
            actual.EmployerAccountId.Should().Be(employerInfo.EmployerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId);
            actual.AccountLegalEntityName.Should().Be(employerInfo.LegalEntities.Last().Name);
            actual.VacancyId.Should().Be(vacancy.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_The_ConfirmLegalEntityViewModel_Is_Returned_For_Existing_Vacancy_Using_Supplied_Employer_Details_When_Provided(
            string employerAccountId,
            string accountLegalEntityPublicHashedId,
            Vacancy vacancy,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);

            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, accountLegalEntityPublicHashedId);

            actual.EmployerAccountId.Should().Be(employerAccountId);
            actual.AccountLegalEntityPublicHashedId.Should().Be(accountLegalEntityPublicHashedId);
            actual.VacancyId.Should().Be(vacancy.Id);
        }

        [Test, MoqAutoData]
        public async Task Then_Back_And_Cancel_Links_Are_Set_Correctly_For_New_Vacancy(
            string employerAccountId,
            string accountLegalEntityPublicHashedId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            vacancyRouteModel.VacancyId = null;
            employerInfo.EmployerAccountId = employerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = accountLegalEntityPublicHashedId;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, accountLegalEntityPublicHashedId);

            actual.CancelLinkRoute.Should().Be(RouteNames.Dashboard_Get);
            actual.BackLinkRoute.Should().Be(RouteNames.LegalEntityEmployer_Get);
        }

        [Test, MoqAutoData]
        public async Task Then_Back_And_Cancel_Links_Point_To_TaskList_For_Existing_Incomplete_Vacancy(
            Vacancy vacancy,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);
            utility.Setup(x => x.IsTaskListCompleted(vacancy)).Returns(false);

            employerInfo.EmployerAccountId = vacancy.EmployerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, null, null);

            actual.CancelLinkRoute.Should().Be(RouteNames.ProviderTaskListGet);
            actual.BackLinkRoute.Should().Be(RouteNames.ProviderTaskListGet);
        }

        [Test, MoqAutoData]
        public async Task Then_Back_And_Cancel_Links_Point_To_CheckYourAnswers_For_Existing_Completed_Vacancy(
            Vacancy vacancy,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.ConfirmLegalEntityEmployer_Get))
                .ReturnsAsync(vacancy);
            utility.Setup(x => x.IsTaskListCompleted(vacancy)).Returns(true);

            employerInfo.EmployerAccountId = vacancy.EmployerAccountId;
            employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId = vacancy.AccountLegalEntityPublicHashedId;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            var actual = await orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, null, null);

            actual.CancelLinkRoute.Should().Be(RouteNames.ProviderCheckYourAnswersGet);
            actual.BackLinkRoute.Should().Be(RouteNames.ProviderCheckYourAnswersGet);
        }

        [Test, MoqAutoData]
        public void Then_MissingPermissionsException_Is_Thrown_When_No_Legal_Entities_Found(
            string employerAccountId,
            string employerAccountLegalEntityId,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            vacancyRouteModel.VacancyId = null;

            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo>());

            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, employerAccountLegalEntityId));
        }

        [Test, MoqAutoData]
        public void Then_MissingPermissionsException_Is_Thrown_When_No_Matching_Legal_Entity_Found(
            string employerAccountId,
            string employerAccountLegalEntityId,
            EmployerInfo employerInfo,
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            vacancyRouteModel.VacancyId = null;

            // employerInfo has a different EmployerAccountId / legalEntityId than the ones passed in
            providerRelationshipsService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(new List<EmployerInfo> { employerInfo });

            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetConfirmLegalEntityViewModel(vacancyRouteModel, employerAccountId, employerAccountLegalEntityId));
        }
    }
}