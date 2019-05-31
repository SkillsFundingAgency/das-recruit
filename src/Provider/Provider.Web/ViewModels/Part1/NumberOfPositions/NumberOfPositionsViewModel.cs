using System;
using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsViewModel : NumberOfPositionsEditModel
    {
        public long Ukprn { get; set; }
        public string Title { get; set; }

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.NumberOfPositions_Post : RouteNames.CreateVacancy_Post;

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions)
        };
    }
}
