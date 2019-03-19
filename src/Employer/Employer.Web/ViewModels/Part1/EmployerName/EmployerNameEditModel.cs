using System.ComponentModel.DataAnnotations;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.EmployerName
{
    public class EmployerNameEditModel : VacancyRouteModel
    {
        public EmployerNameOptionViewModel? SelectedEmployerNameOption { get; set; }
        
        public string NewTradingName { get; set; }
    }
}