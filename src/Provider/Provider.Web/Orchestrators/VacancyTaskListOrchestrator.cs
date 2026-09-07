using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Exceptions;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators;

public class VacancyTaskListOrchestrator(
    IUtility utility,
    IProviderVacancyClient providerVacancyClient,
    IGetProviderStatusClient providerStatusClient,
    ITaskListValidator taskListValidator)
{
    public async Task<VacancyTaskListViewModel> GetVacancyTaskListModel(VacancyRouteModel routeModel)
    {
        var vacancy = await utility.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.ProviderTaskListGet);
        var editVacancyInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(routeModel.Ukprn, vacancy.EmployerAccountId);
        var employerInfoTask = providerVacancyClient.GetProviderEmployerVacancyDataAsync(routeModel.Ukprn, vacancy.EmployerAccountId);
        await Task.WhenAll(editVacancyInfoTask, employerInfoTask);

        
        var taskListStateView = new ProviderTaskListStateView(
            await taskListValidator.GetItemStatesAsync(vacancy, ProviderTaskListSectionFlags.All),
            vacancy
        );

        var providerEditVacancyInfo = editVacancyInfoTask.Result;
        var employerInfo = employerInfoTask.Result;
        
        var providerAccountResponse = await providerStatusClient.GetProviderStatus(routeModel.Ukprn);

        if (!providerAccountResponse.CanAccessService)
        {
            throw new MissingPermissionsException(string.Format(RecruitWebExceptionMessages.ProviderMissingPermission, routeModel.Ukprn));
        }
    
        return new VacancyTaskListViewModel
        {
            AccountCount = providerEditVacancyInfo.Employers.Count(),
            AccountLegalEntityCount = employerInfo?.LegalEntities.Count ?? 0,
            ApplicationMethod = vacancy.ApplicationMethod,
            ApprenticeshipType = vacancy.ApprenticeshipType.GetValueOrDefault(),
            Ukprn = routeModel.Ukprn,
            Status = vacancy.Status,
            TaskListStates = taskListStateView,
            VacancyId = vacancy.Id,
            AdditionalQuestionCount = vacancy.ApprenticeshipType.GetValueOrDefault() == ApprenticeshipTypes.Foundation ? 3 : 4,
        };
    }

    public async Task<VacancyTaskListViewModel> GetCreateVacancyTaskListModel(VacancyRouteModel vrm, string employerAccountId)
    {
        var employerInfoTask = providerVacancyClient.GetProviderEmployerVacancyDataAsync(vrm.Ukprn, employerAccountId);
        var editVacancyInfoTask = providerVacancyClient.GetProviderEditVacancyInfoAsync(vrm.Ukprn, employerAccountId);
        
        await Task.WhenAll(employerInfoTask, editVacancyInfoTask);

        var employerInfo = employerInfoTask.Result;

        return new VacancyTaskListViewModel
        {
            AccountCount = editVacancyInfoTask.Result.Employers.Count(),
            AccountLegalEntityCount = employerInfo?.LegalEntities.Count ?? 0,
            Ukprn = vrm.Ukprn,
            TaskListStates = ProviderTaskListStateView.CreateEmpty(),
            VacancyId = vrm.VacancyId
        };
    }
}