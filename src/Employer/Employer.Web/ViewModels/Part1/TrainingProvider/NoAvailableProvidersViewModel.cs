using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider;

public class NoAvailableProvidersViewModel : VacancyRouteModel
{
    public string CourseTitle { get; init; }
    public bool IsWizard { get; init; }
    public string ProgrammeId { get; set; }
    public string VacancyTitle { get; init; }
}