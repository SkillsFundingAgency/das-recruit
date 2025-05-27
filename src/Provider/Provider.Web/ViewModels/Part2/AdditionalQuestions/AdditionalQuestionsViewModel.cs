using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.AdditionalQuestions;

public class AdditionalQuestionsViewModel : VacancyRouteModel
{
    public string VacancyTitle { get; set; }
    public string AdditionalQuestion1 { get; internal set; }
    public string AdditionalQuestion2 { get; internal set; }
    public ReviewSummaryViewModel Review { get; set; } = new();
    public bool IsTaskListCompleted { get ; set ; }
    public string FindAnApprenticeshipUrl { get; set; }
    public ApprenticeshipTypes ApprenticeshipType { get; set; }
    public int QuestionCount { get; set; }
}