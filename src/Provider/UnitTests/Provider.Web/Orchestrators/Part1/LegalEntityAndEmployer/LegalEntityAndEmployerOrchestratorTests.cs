using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.Orchestrators.Part1;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.LegalEntityAndEmployer;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Testing.AutoFixture;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators.Part1.LegalEntityAndEmployer
{
    public class LegalEntityAndEmployerOrchestratorTests
    {
        [Test, MoqAutoData]
        public void Then_If_There_Are_No_Permissions_Returned_Exception_Thrown(
            VacancyRouteModel vacancyRouteModel,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync((ProviderEditVacancyInfo)null);

            Assert.ThrowsAsync<MissingPermissionsException>(() =>
                orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                    "", 1, SortOrder.Ascending, SortByType.EmployerName));
        }


        [Test, MoqAutoData]
        public async Task Then_If_There_Are_Permissions_Then_The_ViewModel_Is_Returned(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1);

            actual.Employers.Count().Should().Be(providerEditVacancyInfo.Employers.Select(e => new EmployerViewModel { Id = e.EmployerAccountId, Name = e.Name }).Count());
            actual.VacancyId.Should().Be(vacancyRouteModel.VacancyId);
            actual.Ukprn.Should().Be(vacancyRouteModel.Ukprn);
        }


        [Test, MoqAutoData]
        public async Task When_Ordered_By_Employer_Name_in_Descending_Order(
                VacancyRouteModel vacancyRouteModel,
                ProviderEditVacancyInfo providerEditVacancyInfo,
                [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
                LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1, SortOrder.Descending, SortByType.EmployerName);

            actual.Organisations.Should().BeInDescendingOrder(x => x.EmployerName);
            actual.SortByAscDesc.Should().Be(SortOrder.Ascending);
        }

        [Test, MoqAutoData]
        public async Task When_Ordered_By_Employer_Name_in_Ascending_Order(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1, SortOrder.Ascending, SortByType.EmployerName);

            actual.Organisations.Should().BeInAscendingOrder(x => x.EmployerName);
            actual.SortByAscDesc.Should().Be(SortOrder.Descending);
        }

        [Test, MoqAutoData]
        public async Task When_Ordered_By_Legal_Entity_in_Descending_Order(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1, SortOrder.Descending, SortByType.LegalEntityName);

            actual.Organisations.Should().BeInDescendingOrder(x => x.AccountLegalEntityName);
            actual.SortByAscDesc.Should().Be(SortOrder.Ascending);
            actual.Pager.OtherRouteValues["sortOrder"].Should().Be(SortOrder.Descending.ToString());
            actual.Pager.OtherRouteValues["sortByType"].Should().Be(SortByType.LegalEntityName.ToString());
        }

        [Test, MoqAutoData]
        public async Task When_Ordered_By_Legal_Entity_in_Ascending_Order(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "", 1, SortOrder.Ascending, SortByType.LegalEntityName);

            actual.Organisations.Should().BeInAscendingOrder(x => x.AccountLegalEntityName);
            actual.SortByAscDesc.Should().Be(SortOrder.Descending);
            actual.Pager.OtherRouteValues["sortOrder"].Should().Be(SortOrder.Ascending.ToString());
            actual.Pager.OtherRouteValues["sortByType"].Should().Be(SortByType.LegalEntityName.ToString());
        }
        
        
        [Test, MoqAutoData]
        public async Task When_Searching_Then_Results_Are_Returned_And_WhiteSpace_Removed(
            VacancyRouteModel vacancyRouteModel,
            ProviderEditVacancyInfo providerEditVacancyInfo,
            List<EmployerInfo> employerInfo,
            [Frozen] Mock<IProviderVacancyClient> providerVacancyClient,
            LegalEntityAndEmployerOrchestrator orchestrator)
        {
            employerInfo.Add(new EmployerInfo
            {
                Name = "ESFA  LTD",
                LegalEntities = new List<LegalEntity>{new LegalEntity
                {
                    Name = "ESFA  LTD",
                    HasLegalEntityAgreement = true,
                    AccountLegalEntityPublicHashedId = "ABC123"
                }}
            });
            providerEditVacancyInfo.Employers = employerInfo;
            providerVacancyClient.Setup(x => x.GetProviderEmployerVacancyDatasAsync(vacancyRouteModel.Ukprn,
                It.IsAny<IList<string>>())).ReturnsAsync(employerInfo);
            
            providerVacancyClient.Setup(x => x.GetProviderEditVacancyInfoAsync(vacancyRouteModel.Ukprn))
                .ReturnsAsync(providerEditVacancyInfo);

            var actual = await orchestrator.GetLegalEntityAndEmployerViewModelAsync(vacancyRouteModel,
                "ESFA LTD", 1, SortOrder.Descending, SortByType.LegalEntityName);


            actual.Organisations.Should().HaveCount(1);
            actual.Organisations.First().AccountLegalEntityName.Should().Be("ESFA  LTD");
        }
    }


}
