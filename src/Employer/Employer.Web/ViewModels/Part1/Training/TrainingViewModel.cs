using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : TrainingEditModel
    {
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }
        public IList<string> OrderedFieldNames => new List<string>
        {
            nameof(TrainingEditModel.ClosingDate),
            nameof(TrainingEditModel.StartDate),
            nameof(TrainingEditModel.SelectedProgrammeId)
        };

        public PartOnePageInfoViewModel PageInfo { get; set; }
    }

    public class ApprenticeshipProgrammeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
