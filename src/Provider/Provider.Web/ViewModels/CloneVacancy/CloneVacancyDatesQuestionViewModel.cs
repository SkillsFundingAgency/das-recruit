using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.CloneVacancy
{
    public class CloneVacancyDatesQuestionViewModel : VacancyRouteModel
    {        
        public string ClosingDate { get; set; }
        public string StartDate { get; set; } 
        public bool? HasConfirmedClone { get; set; }
    }
}