using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Employer
{
    public class EmployersViewModel : VacancyRouteModel
    {
        public string SelectedEmployerId { get; set; }
        public IEnumerable<EmployerViewModel> Employers { get; set; }
    }
}