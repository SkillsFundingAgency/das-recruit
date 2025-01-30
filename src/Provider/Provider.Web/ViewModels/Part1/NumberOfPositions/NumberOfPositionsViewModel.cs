using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public PartOnePageInfoViewModel PageInfo { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public string NumberOfPositions { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions)
        };

        public string PageBackLink
        {
            get
            {
                return RouteToCheckYourAnswersPage
                    ? RouteNames.ProviderCheckYourAnswersGet
                    : RouteNames.Wage_Get;
            }
        }

        public bool RouteToCheckYourAnswersPage { get; set; }
    }
}
