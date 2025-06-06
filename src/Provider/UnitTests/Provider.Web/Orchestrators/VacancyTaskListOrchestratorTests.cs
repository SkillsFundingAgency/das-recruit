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