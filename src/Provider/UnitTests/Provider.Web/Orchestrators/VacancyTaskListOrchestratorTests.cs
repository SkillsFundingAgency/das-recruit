using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators
{
    public class VacancyTaskListOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Is_Retrieved_And_Mapped(
            string findAnApprenticeshipUrl,
            VacancyRouteModel routeModel,
            ApprenticeshipProgramme programme,
            Vacancy vacancy,
            List<LegalEntity> legalEntities,
            [Frozen] Mock<IOptions<ExternalLinksConfiguration>> externalLinksConfiguration,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            VacancyTaskListOrchestrator orchestrator)
        {
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.ClosedDate = null;
            programme.Id = vacancy.ProgrammeId;
            programme.EducationLevelNumber = 3;
            programme.ApprenticeshipLevel = ApprenticeshipLevel.Higher;
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(routeModel.Ukprn,
                vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId,
                OperationType.RecruitmentRequiresReview)).ReturnsAsync(false);
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(
                    c => c.VacancyId.Equals(routeModel.VacancyId) &&
                         c.Ukprn.Equals(routeModel.Ukprn)), RouteNames.ProviderTaskListGet))
                .ReturnsAsync(vacancy);
            recruitVacancyClient.Setup(x => x.GetActiveApprenticeshipProgrammesAsync())
                .ReturnsAsync(new List<ApprenticeshipProgramme>{ programme});
            recruitVacancyClient.Setup(x => x.GetApprenticeshipProgrammeAsync(programme.Id))
                .ReturnsAsync(programme);
            recruitVacancyClient.Setup(x => x.GetEmployerDescriptionAsync(vacancy)).ReturnsAsync(vacancy.EmployerDescription);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(vacancy.EmployerName);
            externalLinksConfiguration.Object.Value.FindAnApprenticeshipUrl = findAnApprenticeshipUrl;
            var expectedViewModel = new VacancyPreviewViewModel();
            var mapper = new DisplayVacancyViewModelMapper(Mock.Of<IGeocodeImageService>(),
                externalLinksConfiguration.Object, recruitVacancyClient.Object, providerVacancyClient.Object);

            var viewModel = await orchestrator.GetVacancyTaskListModel(routeModel);

            await mapper.MapFromVacancyAsync(expectedViewModel, vacancy);
            viewModel.Should().BeAssignableTo<VacancyPreviewViewModel>();
            viewModel.Should().BeEquivalentTo(expectedViewModel, options=>options
                .Excluding(c=>c.SoftValidationErrors)
                .Excluding(c=>c.HasProgramme)
                .Excluding(c=>c.HasWage)
                .Excluding(c=>c.CanShowReference)
                .Excluding(c=>c.CanShowDraftHeader)
                .Excluding(c=>c.EducationLevelName)
                .Excluding(c=>c.ApprenticeshipLevel)
                .Excluding(c=>c.AccountLegalEntityCount)
                .Excluding(c=>c.Ukprn)
                .Excluding(c=>c.VacancyId)
                .Excluding(c=>c.RouteDictionary)
                .Excluding(c=>c.HasSelectedEmployerNameOption)
                .Excluding(c=>c.HasSoftValidationErrors)
            );
            viewModel.ApprenticeshipLevel.Should().Be(programme.ApprenticeshipLevel);
            viewModel.HasSelectedEmployerNameOption.Should().BeTrue();
            viewModel.Ukprn.Should().Be(routeModel.Ukprn);
            viewModel.VacancyId.Should().Be(routeModel.VacancyId);
        }

        [Test, MoqAutoData]
        public async Task Then_If_Traineeship_Then_Filtered_By_Employers_Without_Review_Permission(
            VacancyRouteModel routeModel,
            ApprenticeshipProgramme programme,
            EmployerInfo employerInfo,
            Vacancy vacancy,
            List<LegalEntity> legalEntities,
            [Frozen] Mock<IOptions<ExternalLinksConfiguration>> externalLinksConfiguration,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            VacancyTaskListOrchestrator orchestrator)
        {
            var expectedLegalEntityCount = employerInfo.LegalEntities.Count - 1;
            vacancy.VacancyType = VacancyType.Traineeship;
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancy.EmployerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService
                .Setup(x => x.GetAccountLegalEntitiesForProvider(routeModel.Ukprn, vacancy.EmployerAccountId,
                    OperationType.RecruitmentRequiresReview)).ReturnsAsync(new List<LegalEntity>
                {
                    new LegalEntity
                    {
                        AccountLegalEntityPublicHashedId = employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId
                    }
                });
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(
                    c => c.VacancyId.Equals(routeModel.VacancyId) &&
                         c.Ukprn.Equals(routeModel.Ukprn)), RouteNames.ProviderTaskListGet))
                .ReturnsAsync(vacancy);
            
            var viewModel = await orchestrator.GetVacancyTaskListModel(routeModel);
        
            viewModel.AccountLegalEntityCount.Should().Be(expectedLegalEntityCount);
        }
        
        [Test, MoqAutoData]
        public async Task Then_If_Apprenticeship_Then_Not_Filtered_By_Employers_Without_Review_Permission(
            VacancyRouteModel routeModel,
            ApprenticeshipProgramme programme,
            EmployerInfo employerInfo,
            Vacancy vacancy,
            List<LegalEntity> legalEntities,
            [Frozen] Mock<IOptions<ExternalLinksConfiguration>> externalLinksConfiguration,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            VacancyTaskListOrchestrator orchestrator)
        {
            var expectedLegalEntityCount = employerInfo.LegalEntities.Count;
            vacancy.VacancyType = VacancyType.Apprenticeship;
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancy.EmployerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService
                .Setup(x => x.GetAccountLegalEntitiesForProvider(routeModel.Ukprn, vacancy.EmployerAccountId,
                    OperationType.RecruitmentRequiresReview)).ReturnsAsync(new List<LegalEntity>
                {
                    new LegalEntity
                    {
                        AccountLegalEntityPublicHashedId = employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId
                    }
                });
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(
                    c => c.VacancyId.Equals(routeModel.VacancyId) &&
                         c.Ukprn.Equals(routeModel.Ukprn)), RouteNames.ProviderTaskListGet))
                .ReturnsAsync(vacancy);
            
            var viewModel = await orchestrator.GetVacancyTaskListModel(routeModel);
        
            viewModel.AccountLegalEntityCount.Should().Be(expectedLegalEntityCount);
            providerRelationshipsService
                .Verify(x => x.GetAccountLegalEntitiesForProvider(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<OperationType>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task When_Creating_New_Then_The_Account_Legal_Entity_Count_Is_Populated_For_Apprenticeship(
            VacancyRouteModel routeModel,
            EmployerInfo employerInfo,
            string employerAccountId,
            [Frozen] DisplayVacancyViewModelMapper mapper,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient)
        {
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);

            var orchestrator = new VacancyTaskListOrchestrator(Mock.Of<ILogger<VacancyTaskListOrchestrator>>(),
                mapper, Mock.Of<IUtility>(), providerVacancyClient.Object,
                Mock.Of<IRecruitVacancyClient>(), Mock.Of<IReviewSummaryService>(), providerRelationshipsService.Object, new ServiceParameters("Apprenticeship"));
            
            var viewModel = await orchestrator.GetCreateVacancyTaskListModel(routeModel, employerAccountId);
        
            viewModel.AccountLegalEntityCount.Should().Be(employerInfo.LegalEntities.Count);
            providerRelationshipsService
                .Verify(x => x.GetAccountLegalEntitiesForProvider(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<OperationType>()), Times.Never);
        }
        
        [Test, MoqAutoData]
        public async Task When_Creating_New_Then_The_Account_Legal_Entity_Count_Is_Populated_For_Traineeship(
            VacancyRouteModel routeModel,
            EmployerInfo employerInfo,
            string employerAccountId,
            [Frozen] DisplayVacancyViewModelMapper mapper,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService)
        {
            var expectedLegalEntityCount = employerInfo.LegalEntities.Count - 1;
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerRelationshipsService
                .Setup(x => x.GetAccountLegalEntitiesForProvider(routeModel.Ukprn, employerAccountId,
                    OperationType.RecruitmentRequiresReview)).ReturnsAsync(new List<LegalEntity>
                {
                    new LegalEntity
                    {
                        AccountLegalEntityPublicHashedId = employerInfo.LegalEntities.Last().AccountLegalEntityPublicHashedId
                    }
                });
            var orchestrator = new VacancyTaskListOrchestrator(Mock.Of<ILogger<VacancyTaskListOrchestrator>>(),
                mapper, Mock.Of<IUtility>(), providerVacancyClient.Object,
                Mock.Of<IRecruitVacancyClient>(), Mock.Of<IReviewSummaryService>(), providerRelationshipsService.Object, new ServiceParameters("Traineeship"));
            
            var viewModel = await orchestrator.GetCreateVacancyTaskListModel(routeModel, employerAccountId);
        
            viewModel.AccountLegalEntityCount.Should().Be(expectedLegalEntityCount);
        }
    }
}