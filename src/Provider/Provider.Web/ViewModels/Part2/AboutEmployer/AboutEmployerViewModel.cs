using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.AboutEmployer
{
    public class AboutEmployerViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string EmployerDescription { get; internal set; }
        public string EmployerTitle { get; internal set; }
        public string EmployerWebsiteUrl { get; internal set; }
        public bool IsAnonymous { get; internal set; }
        public bool IsDisabilityConfident { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(EmployerDescription),
            nameof(EmployerWebsiteUrl)
        };
    }
}
