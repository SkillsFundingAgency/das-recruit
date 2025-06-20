using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels;

public class VacancyTaskListViewModel: VacancyRouteModel
{
    public int AccountLegalEntityCount { get ; set ; }
    public ApprenticeshipTypes ApprenticeshipType { get; set; }
    public ApplicationMethod? ApplicationMethod { get; set; }
    public VacancyStatus Status { get; set; }
    public EmployerTaskListStateView TaskListStates { get; set; }
}