using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Jobs.TrainingTypes.Models
{
    public class ApprenticeshipProgrammeView
    {
        public string Id => "TrainingTypes";
        public IEnumerable<ApprenticeshipProgramme> ApprenticeshipProgrammes { get; internal set; } 
    }
}
