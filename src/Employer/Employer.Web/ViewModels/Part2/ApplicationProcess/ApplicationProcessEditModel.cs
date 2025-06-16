using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.ApplicationProcess;

public class ApplicationProcessEditModel : TaskListViewModel
{
    public ApplicationMethod? ApplicationMethod { get; set; }
    public string ApplicationInstructions { get; set; }
    public string ApplicationUrl { get; set; }
}