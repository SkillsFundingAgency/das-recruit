using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;

public class AdditionalQuestionsViewModel : VacancyRouteModel
{
    public string AdditionalQuestion1 { get; internal set; }
    public string AdditionalQuestion2 { get; internal set; }
    public ReviewSummaryViewModel Review { get; set; } = new();
    public bool IsTaskListCompleted { get ; set ; }
    public string? FindAnApprenticeshipUrl { get; set; }
}