using System.Collections.Generic;
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
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
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
            VacancyTaskListOrchestrator orchestrator)
        {
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.ClosedDate = null;
            programme.Id = vacancy.ProgrammeId;
            programme.EducationLevelNumber = 3;
            programme.ApprenticeshipLevel = ApprenticeshipLevel.Higher;
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
            );
            viewModel.ApprenticeshipLevel.Should().Be(programme.ApprenticeshipLevel);
            viewModel.HasSelectedEmployerNameOption.Should().BeTrue();
            viewModel.Ukprn.Should().Be(routeModel.Ukprn);
            viewModel.VacancyId.Should().Be(routeModel.VacancyId);
        }

        [Test, MoqAutoData]
        public async Task When_Creating_New_Then_The_Account_Legal_Entity_Count_Is_Populated(
            VacancyRouteModel routeModel,
            EmployerInfo employerInfo,
            string employerAccountId,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            VacancyTaskListOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            
            var viewModel = await orchestrator.GetCreateVacancyTaskListModel(routeModel, employerAccountId);
        
            viewModel.AccountLegalEntityCount.Should().Be(employerInfo.LegalEntities.Count);
        }
    }
}