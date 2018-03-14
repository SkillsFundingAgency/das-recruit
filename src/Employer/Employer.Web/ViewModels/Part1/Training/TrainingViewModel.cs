using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part1.Training
{
    public class TrainingViewModel : TrainingEditModel
    {
        public IEnumerable<ApprenticeshipProgrammeViewModel> Programmes { get; set; }
    }

    public class ApprenticeshipProgrammeViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
