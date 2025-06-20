using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;
using Microsoft.Extensions.Logging;
using ErrorMessages = Esfa.Recruit.Shared.Web.ViewModels.ErrorMessages;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class VacancyTaskListOrchestrator(
    IUtility utility,
    IProviderVacancyClient providerVacancyClient,
    ITaskListValidator taskListValidator)
{
    public async Task<VacancyTaskListViewModel> GetVacancyTaskListModel(VacancyRouteModel routeModel)
    {
        var vacancyTask = utility.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.ProviderTaskListGet);
        var editVacancyInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(routeModel.Ukprn);
        var employerInfoTask = providerVacancyClient.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancyTask.Result.EmployerAccountId);
        await Task.WhenAll(vacancyTask, editVacancyInfoTask, employerInfoTask);

        var vacancy = vacancyTask.Result;
        var taskListStateView = new ProviderTaskListStateView(
            await taskListValidator.GetItemStatesAsync(vacancy, ProviderTaskListSectionFlags.All),
            vacancy
        );
        
        return new VacancyTaskListViewModel
        {
            AccountCount = editVacancyInfoTask.Result.Employers.Count(),
            AccountLegalEntityCount = employerInfoTask.Result.LegalEntities.Count,
            ApplicationMethod = vacancy.ApplicationMethod,
            ApprenticeshipType = vacancy.ApprenticeshipType.GetValueOrDefault(),
            Ukprn = routeModel.Ukprn,
            Status = vacancy.Status,
            TaskListStates = taskListStateView,
            VacancyId = vacancy.Id
        };
    }

    public async Task<VacancyTaskListViewModel> GetCreateVacancyTaskListModel(VacancyRouteModel vrm, string employerAccountId)
    {
        var employerInfoTask = providerVacancyClient.GetProviderEmployerVacancyDataAsync(vrm.Ukprn, employerAccountId);
        var editVacancyInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn);
        
        await Task.WhenAll(employerInfoTask, editVacancyInfoTask);

        return new VacancyTaskListViewModel
        {
            AccountCount = editVacancyInfoTask.Result.Employers.Count(),
            AccountLegalEntityCount = employerInfoTask.Result.LegalEntities.Count,
            Ukprn = vrm.Ukprn,
            TaskListStates = ProviderTaskListStateView.CreateEmpty(),
            VacancyId = vrm.VacancyId
        };
    }
}