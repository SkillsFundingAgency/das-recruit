using System;
using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : TrainingEditModel
    {
        public int MinYear => DateTime.UtcNow.Year;
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }
    }

    public class ApprenticeshipProgrammeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
