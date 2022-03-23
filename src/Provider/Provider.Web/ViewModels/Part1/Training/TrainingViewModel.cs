using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : VacancyRouteModel
    {
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }

        public string SelectedProgrammeId { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(SelectedProgrammeId)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
        public bool HasMoreThanOneLegalEntity { get; set; }
    }
}