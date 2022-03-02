using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.TrainingProvider
{
    public class SelectTrainingProviderEditModel : VacancyRouteModel
    {
        public string Ukprn { get; set; }

        public string TrainingProviderSearch { get; set; }

        public TrainingProviderSelectionType SelectionType { get; set; }
    }

    public enum TrainingProviderSelectionType
    {
        Ukprn,
        TrainingProviderSearch
    }
}