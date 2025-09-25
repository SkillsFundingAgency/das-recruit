using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels;

public class VacancyTaskListViewModel: VacancyRouteModel
{
    public int AccountCount { get ; set ; }
    public int AccountLegalEntityCount { get ; set ; }
    public ApprenticeshipTypes ApprenticeshipType { get; set; }
    public ApplicationMethod? ApplicationMethod { get; set; }
    public VacancyStatus Status { get; set; }
    public ProviderTaskListStateView TaskListStates { get; set; }
    public int AdditionalQuestionCount { get; set; }
}