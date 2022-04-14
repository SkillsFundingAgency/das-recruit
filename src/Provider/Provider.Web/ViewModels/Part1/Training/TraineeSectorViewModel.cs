using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Training
{
    public class TraineeSectorViewModel : VacancyRouteModel
    {
        public string Title { get; set; }
        public IEnumerable<ApprenticeshipRouteViewModel> Routes { get; set; }

        public int? SelectedRouteId { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(SelectedRouteId)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public bool IsTaskListCompleted { get; set; }
    }
}