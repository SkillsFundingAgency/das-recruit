using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels
{
    public class CloseViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string VacancyReference { get; internal set; }
        public bool? ConfirmClose { get; set; }
    }
}