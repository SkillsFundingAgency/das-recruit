using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Qualifications;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Qualifications
{
    public class QualificationsViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public List<QualificationEditModel> Qualifications { get; set; }
        public string InfoMessage { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public bool HasInfo => string.IsNullOrWhiteSpace(InfoMessage) == false;
    }
}