using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Employer.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.ApprenticeshipProgrammes;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators
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
            [Frozen] Mock<IEmployerVacancyClient> employerVacancyClient,
            VacancyTaskListOrchestrator orchestrator)
        {
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            programme.Id = vacancy.ProgrammeId;
            programme.EducationLevelNumber = 3;
            programme.ApprenticeshipLevel = ApprenticeshipLevel.Higher;
            employerVacancyClient.Setup(x => x.GetEmployerLegalEntitiesAsync(routeModel.EmployerAccountId))
                .ReturnsAsync(legalEntities);
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(
                    c => c.VacancyId.Equals(routeModel.VacancyId) &&
                         c.EmployerAccountId.Equals(routeModel.EmployerAccountId)), RouteNames.EmployerTaskListGet))
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
                externalLinksConfiguration.Object, recruitVacancyClient.Object);

            var viewModel = await orchestrator.GetVacancyTaskListModel(routeModel);

            await mapper.MapFromVacancyAsync(expectedViewModel, vacancy);
            
            viewModel.Should().BeEquivalentTo(expectedViewModel, options=>options
                .Excluding(c=>c.SoftValidationErrors)
                .Excluding(c=>c.RejectedReason)
                .Excluding(c=>c.HasProgramme)
                .Excluding(c=>c.HasWage)
                .Excluding(c=>c.CanShowReference)
                .Excluding(c=>c.CanShowDraftHeader)
                .Excluding(c=>c.EducationLevelName)
                .Excluding(c=>c.ApprenticeshipLevel)
                .Excluding(c=>c.AccountLegalEntityCount)
                .Excluding(c=>c.HasSelectedEmployerNameOption)
            );
            viewModel.ApprenticeshipLevel.Should().Be(programme.ApprenticeshipLevel);
            viewModel.AccountLegalEntityCount.Should().Be(legalEntities.Count);
            viewModel.HasSelectedEmployerNameOption.Should().BeTrue();
        }
    }
}