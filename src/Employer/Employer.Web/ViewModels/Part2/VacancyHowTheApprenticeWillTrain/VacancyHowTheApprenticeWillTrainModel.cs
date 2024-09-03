using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.VacancyHowTheApprenticeWillTrain;

public class VacancyHowTheApprenticeWillTrainModel : VacancyRouteModel
{
    public string Title { get; internal set; }
    public string TrainingDescription { get; set; }
    public string AdditionalTrainingDescription { get; set; }
    public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
    public IList<string> OrderedFieldNames => new List<string>
    {
        nameof(VacancyHowTheApprenticeWillTrainModel.TrainingDescription),
        nameof(VacancyHowTheApprenticeWillTrainModel.AdditionalTrainingDescription)
    };

    public bool IsTaskListCompleted { get ; set ; }
}