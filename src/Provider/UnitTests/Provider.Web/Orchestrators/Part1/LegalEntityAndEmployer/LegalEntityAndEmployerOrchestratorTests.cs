using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Vacancies.Client.Application.FeatureToggle;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1.LegalEntityAndEmployer
{
    public class LegalEntityAndEmployerOrchestratorTests
    {
        [Test, MoqAutoData]
        public void Then_If_There_Are_No_Permissions_Returned_Exception_Thrown(
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipsService,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerRelationshipsService.Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    It.IsAny<List<OperationType>>()))
                .ReturnsAsync(new List<EmployerInfo>());

            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                    "", 1, SortOrder.Ascending, SortByType.EmployerName));
        }


        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Permissions_Then_The_ViewModel_Is_Returned(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            [Frozen] Mock<IFeature> feature,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigrationEmployerProfiles)).Returns(true);
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn, ""))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1);

            actual.Employers.Count().Should().Be(providerEditVacancyInfo.Employers
                .Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name }).Count());
            actual.VacancyId.Should().Be(vacancyRouteModel.VacancyId);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }

        [Test, MoqAutoData]
        public async Task Then_Feature_Enabled_Then_There_Are_Permissions_Then_The_ViewModel_Is_Returned(
            VacancyRouteModel vacancyRouteModel,
            GetAllAccountLegalEntitiesApiResponse response,
            IEnumerable<EmployerInfo> mockEmployerInfos,
            [Frozen] Mock<IEmployerAccountProvider> employerAccountProvider,
            [Frozen] Mock<IProviderRelationshipsService> providerRelationshipService,
            [Frozen] Mock<IFeature> feature,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            feature.Setup(x => x.IsFeatureEnabled(FeatureNames.MongoMigrationEmployerProfiles)).Returns(true);

            employerAccountProvider.Setup(x => x.GetAllLegalEntitiesConnectedToAccountAsync(It.IsAny<List<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<bool>()))
                .ReturnsAsync(response);

            providerRelationshipService
                .Setup(x => x.GetLegalEntitiesForProvider(vacancyRouteModel.Ukprn, "",
                    new List<OperationType> { OperationType.Recruitment, OperationType.RecruitmentRequiresReview }))
                .ReturnsAsync(mockEmployerInfos);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1);

            actual.Employers.Count().Should().Be(response.LegalEntities.Select(e => new EmployerViewModel
                { Id = e.AccountLegalEntityPublicHashedId, Name = e.Name }).Count());
            actual.VacancyId.Should().Be(vacancyRouteModel.VacancyId);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }
    }
}
