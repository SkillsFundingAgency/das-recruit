using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Dashboard
{
    public class WithdrawnVacanciesAlertViewModel
    {
        public List<string> ClosedVacancies { get; set; }
        
        public int ClosedVacanciesCount => ClosedVacancies.Count;

        public bool HasMultipleClosedVacancies => ClosedVacancies.Count > 1;
    }
}
