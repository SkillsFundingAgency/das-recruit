using System.Collections.Generic;
using System.Linq;
using AutoFixture.NUnit3;
using Esfa.Recruit.Employer.Web.Orchestrators;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.Orchestrators;

public class VacancyTaskListOrchestratorTests
{
    [Test, MoqAutoData]
    public async Task When_Creating_New_Then_The_Account_Legal_Entity_Count_Is_Populated(
        VacancyRouteModel routeModel,
        List<LegalEntity> legalEntities,
        [Frozen] Vacancy vacancy,
        [Frozen] Mock<IEmployerVacancyClient> employerVacancyClient,
        [Frozen] Mock<ITaskListValidator> taskListValidator,
        VacancyTaskListOrchestrator orchestrator)
    {
        // arrange
        employerVacancyClient
            .Setup(x => x.GetEmployerLegalEntitiesAsync(routeModel.EmployerAccountId))
            .ReturnsAsync(legalEntities);
        taskListValidator
            .Setup(x => x.GetItemStatesAsync(vacancy, EmployerTaskListSectionFlags.All))
            .ReturnsAsync(Enum.GetValues<TaskListItemFlags>().ToDictionary(flag => flag, _ => false));
            
        // act
        var viewModel = await orchestrator.GetVacancyTaskListModel(routeModel);

        // assert
        viewModel.AccountLegalEntityCount.Should().Be(legalEntities.Count);
    }
        
    [Test, MoqAutoData]
    public async Task When_Calling_GetCreateVacancyTaskListModel_Then_Returns_Count(
        VacancyRouteModel routeModel,
        EmployerEditVacancyInfo responseFromClient,
        [Frozen] Mock<IEmployerVacancyClient> mockEmployerVacancyClient,
        VacancyTaskListOrchestrator orchestrator)
    {
        mockEmployerVacancyClient
            .Setup(x => x.GetEditVacancyInfoAsync(routeModel.EmployerAccountId))
            .ReturnsAsync(responseFromClient);
            
        var viewModel = await orchestrator.GetCreateVacancyTaskListModel(routeModel);

        viewModel.AccountLegalEntityCount.Should().Be(responseFromClient.LegalEntities.Count());
    }
        
    [Test, MoqAutoData]
    public async Task When_Calling_GetCreateVacancyTaskListModel_And_Response_Null_Then_Returns_0_Count(
        VacancyRouteModel routeModel,
        [Frozen] Mock<IEmployerVacancyClient> mockEmployerVacancyClient,
        VacancyTaskListOrchestrator orchestrator)
    {
        mockEmployerVacancyClient
            .Setup(x => x.GetEditVacancyInfoAsync(routeModel.EmployerAccountId))
            .ReturnsAsync((EmployerEditVacancyInfo)null);
            
        var viewModel = await orchestrator.GetCreateVacancyTaskListModel(routeModel);

        viewModel.AccountLegalEntityCount.Should().Be(0);
    }
}