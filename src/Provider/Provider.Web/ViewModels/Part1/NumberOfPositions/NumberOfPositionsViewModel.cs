using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.NumberOfPositions
{
    public class NumberOfPositionsViewModel : NumberOfPositionsEditModel
    {
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(NumberOfPositions)
        };
        public string FormPostRouteName => VacancyId.HasValue ? RouteNames.NumberOfPositions_Post : RouteNames.CreateVacancy_Post;
        public PartOnePageInfoViewModel PageInfo { get; set; }
    }
}
