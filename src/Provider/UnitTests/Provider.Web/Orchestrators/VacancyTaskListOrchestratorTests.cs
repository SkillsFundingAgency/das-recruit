using System.Linq;
using Esfa.Recruit.Provider.Web.Orchestrators;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using NUnit.Framework;

namespace Esfa.Recruit.Provider.UnitTests.Provider.Web.Orchestrators;

public class VacancyTaskListOrchestratorTests
{
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
}