using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators;

public class VacancyTaskListOrchestrator(
    IUtility utility,
    IEmployerVacancyClient employerVacancyClient,
    ITaskListValidator taskListValidator)
{
    public async Task<VacancyTaskListViewModel> GetVacancyTaskListModel(VacancyRouteModel vrm)
    {
        var getEmployerDataTask = employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.EmployerTaskListGet);

        var taskListStateView = new EmployerTaskListStateView(
            await taskListValidator.GetItemStatesAsync(vacancy, EmployerTaskListSectionFlags.All),
            vacancy
        );

        var employerData = await getEmployerDataTask;
        return new VacancyTaskListViewModel
        {
            AccountLegalEntityCount = employerData.LegalEntities.Count(),
            ApprenticeshipType = vacancy.ApprenticeshipType.GetValueOrDefault(),
            EmployerAccountId = vrm.EmployerAccountId,
            Status = vacancy.Status,
            TaskListStates = taskListStateView,
            VacancyId = vacancy.Id
        };
    }

    public async Task<VacancyTaskListViewModel> GetCreateVacancyTaskListModel(VacancyRouteModel vrm)
    {
        var employerData = await employerVacancyClient.GetEditVacancyInfoAsync(vrm.EmployerAccountId);
        return new VacancyTaskListViewModel
        {
            AccountLegalEntityCount = employerData?.LegalEntities?.Count() ?? 0,
            EmployerAccountId = vrm.EmployerAccountId,
            TaskListStates = EmployerTaskListStateView.CreateEmpty()
        };
    }
}