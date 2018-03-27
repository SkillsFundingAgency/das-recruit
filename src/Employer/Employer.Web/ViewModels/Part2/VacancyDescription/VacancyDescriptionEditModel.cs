using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class VacancyDescriptionEditModel : VacancyRouteModel
    {
        public string VacancyDescription { get; set; }
        public string TrainingDescription { get; set; }
        public string OutcomeDescription { get; set; }
    }
}
