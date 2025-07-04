﻿using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Considerations
{
    public class ConsiderationsViewModel : VacancyRouteModel
    {
        public string Title { get; internal set; }
        public string OtherRequirements { get; internal set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public bool IsTaskListCompleted { get; set; }

        public string PageBackLink
        {
            get
            {
                return IsTaskListCompleted
                    ? RouteNames.ProviderCheckYourAnswersGet
                    : RouteNames.FutureProspects_Get;
            }
        }
    }
}
