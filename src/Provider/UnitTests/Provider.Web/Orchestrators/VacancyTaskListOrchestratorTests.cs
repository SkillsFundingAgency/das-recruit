using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web;
using Esfa.Recruit.Provider.Web.Configuration;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.VacancyPreview;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Options;
using NUnit.Framework;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators
{
    public class VacancyTaskListOrchestratorTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Vacancy_Is_Retrieved_And_Mapped(
            string findAnApprenticeshipUrl,
            int standardId,
            VacancyRouteModel routeModel,
            Vacancy vacancy,
            List<LegalEntity> legalEntities,
            List<EmployerInfo> providerEditVacancyInfo,
            EmployerInfo employerInfo,
            ApprenticeshipStandard standard,
            [Frozen] Mock<IOptions<ExternalLinksConfiguration>> externalLinksConfiguration,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            [Frozen] Mock<IApprenticeshipProgrammeProvider> apprenticeshipProgrammeProvider,
            VacancyTaskListOrchestrator orchestrator)
        {
            vacancy.EmployerLocation = null;
            vacancy.EmployerNameOption = EmployerNameOption.RegisteredName;
            vacancy.ClosedDate = null;
            vacancy.ProgrammeId = standardId.ToString();
            standard.EducationLevelNumber = 3;
            standard.ApprenticeshipLevel = ApprenticeshipLevel.Higher;
            providerRelationshipsService.Setup(x => x.HasProviderGotEmployersPermissionAsync(routeModel.Ukprn,
                vacancy.EmployerAccountId, vacancy.AccountLegalEntityPublicHashedId,
                OperationType.RecruitmentRequiresReview)).ReturnsAsync(false);
            utility.Setup(x => x.GetAuthorisedVacancyForEditAsync(It.Is<VacancyRouteModel>(
                    c => c.VacancyId.Equals(routeModel.VacancyId) &&
                         c.Ukprn.Equals(routeModel.Ukprn)), RouteNames.ProviderTaskListGet))
                .ReturnsAsync(vacancy);
            apprenticeshipProgrammeProvider.Setup(x => x.GetApprenticeshipStandardVacancyPreviewData(standardId))
                .ReturnsAsync(standard);
            recruitVacancyClient.Setup(x => x.GetEmployerDescriptionAsync(vacancy)).ReturnsAsync(vacancy.EmployerDescription);
            recruitVacancyClient.Setup(x => x.GetEmployerNameAsync(vacancy)).ReturnsAsync(vacancy.EmployerName);
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancy.EmployerAccountId))
                .ReturnsAsync(employerInfo);
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(routeModel.Ukprn)).ReturnsAsync(new ProviderEditVacancyInfo
            {
                Employers = providerEditVacancyInfo
            });
            externalLinksConfiguration.Object.Value.FindAnApprenticeshipUrl = findAnApprenticeshipUrl;
            var expectedViewModel = new VacancyPreviewViewModel();
            var mapper = new DisplayVacancyViewModelMapper(Mock.Of<IGeocodeImageService>(),
                externalLinksConfiguration.Object,
                recruitVacancyClient.Object,
                providerVacancyClient.Object,
                apprenticeshipProgrammeProvider.Object,
                Mock.Of<IFeature>());

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
                .Excluding(c=>c.AccountCount)
                .Excluding(c=>c.Qualifications)
                .Excluding(c=>c.QualificationsDesired)
                .Excluding(c=>c.HasOptedToAddQualifications)
            );
            viewModel.ApprenticeshipLevel.Should().Be(standard.ApprenticeshipLevel);
            viewModel.HasSelectedEmployerNameOption.Should().BeTrue();
            viewModel.Ukprn.Should().Be(routeModel.Ukprn);
            viewModel.VacancyId.Should().Be(routeModel.VacancyId);
            viewModel.AccountLegalEntityCount.Should().Be(employerInfo.LegalEntities.Count);
            viewModel.AccountCount.Should().Be(providerEditVacancyInfo.Count);
        }

        [Test, MoqAutoData]
        public async Task When_Creating_New_Then_The_Account_Legal_Entity_And_Employer_Count_Is_Populated(
            VacancyRouteModel routeModel,
            EmployerInfo employerInfo,
            string employerAccountId,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            VacancyTaskListOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, employerAccountId))
                .ReturnsAsync(employerInfo);
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(routeModel.Ukprn)).ReturnsAsync(providerEditVacancyInfo);
            
            var viewModel = await orchestrator.GetCreateVacancyTaskListModel(routeModel, employerAccountId);
        
            viewModel.AccountLegalEntityCount.Should().Be(employerInfo.LegalEntities.Count);
            viewModel.AccountCount.Should().Be(providerEditVacancyInfo.Employers.Count());
        }
        
        [Test, MoqAutoData]
        public async Task SubmitVacancyAsync_Updates_Vacancy_Addresses(
            Address address1,
            Address address2,
            Address address3,
            Vacancy vacancy,
            VacancyUser user,
            [Frozen] Mock<IRecruitVacancyClient> recruitVacancyClient,
            [Frozen] Mock<IUtility> utility,
            [Frozen] Mock<ILocationsService> locationsService,
            [Greedy] VacancyTaskListOrchestrator sut
            )
        {
            // arrange
            var submitEditModel = new SubmitEditModel();
            
            address1.Country = null;
            address2.Country = null;
            address3.Country = null;
            
            vacancy.EmployerLocations = [address1, address2, address3];
            vacancy.EmployerLocationOption = AvailableWhere.MultipleLocations;
            vacancy.Status = VacancyStatus.Draft;
            vacancy.IsDeleted = false;

            var postcodeLookupResults = new Dictionary<string, PostcodeData>
            {
                { address1.Postcode, new PostcodeData(address1.Postcode, "England", 1, 1) },
                { address2.Postcode, null },
                { address3.Postcode, new PostcodeData(address1.Postcode, "Northern Ireland", 2, 2) },
            };
            
            utility.Setup(x => x.GetAuthorisedVacancyAsync(submitEditModel, It.IsAny<string>())).ReturnsAsync(vacancy);
            locationsService.Setup(x => x.GetBulkPostcodeDataAsync(It.IsAny<List<string>>())).ReturnsAsync(postcodeLookupResults);
            
            // arrange
            await sut.SubmitVacancyAsync(submitEditModel, user);
            
            // assert
            recruitVacancyClient.Verify(x => x.UpdateDraftVacancyAsync(vacancy, user), Times.Once);
            
            vacancy.EmployerLocations[0].Country.Should().Be("England");
            vacancy.EmployerLocations[1].Country.Should().Be(null);
            vacancy.EmployerLocations[2].Country.Should().Be("Northern Ireland");
        }
    }
}