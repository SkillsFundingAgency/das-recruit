using System;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.CloneVacancy
{
    public class CloneVacancyDatesQuestionViewModel : VacancyRouteModel
    {     
        public long VacancyReference { get; set; }
        public string ClosingDate { get; set; }
        public string StartDate { get; set; } 
        public bool? HasConfirmedClone { get; set; }
    }
}