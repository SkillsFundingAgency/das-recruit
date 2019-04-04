using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.EmployerName
{
    public class EmployerNameEditModel : VacancyRouteModel
    {
        public EmployerNameOptionViewModel? SelectedEmployerNameOption { get; set; }
        
        public string NewTradingName { get; set; }
    }
}