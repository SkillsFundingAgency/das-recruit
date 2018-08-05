using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.VacancyManage
{
    public class ProposedChangesViewModel : VacancyRouteModel
    {
        public string ProposedClosingDate { get; internal set; }

        public string ProposedStartDate { get; internal set; }
    }
}
