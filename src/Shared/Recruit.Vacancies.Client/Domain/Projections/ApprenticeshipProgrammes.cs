using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class ApprenticeshipProgrammes : QueryProjectionBase
    {
        public IEnumerable<ApprenticeshipProgramme> Programmes { get; set; } 
    }
}