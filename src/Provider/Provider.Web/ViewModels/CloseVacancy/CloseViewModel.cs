using Esfa.Recruit.Provider.Web.RouteModel;

namespace Esfa.Recruit.Provider.Web.ViewModels
{
    public class CloseViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string VacancyReference { get; internal set; }
        public bool? ConfirmClose { get; set; }
    }
}