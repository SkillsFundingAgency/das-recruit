using System;
using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : TrainingEditModel
    {
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }

        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();

        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(ClosingDate),
            nameof(StartDate),
            nameof(SelectedProgrammeId)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }

        public int CurrentYear { get; set; }
    }

    public class ApprenticeshipProgrammeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
