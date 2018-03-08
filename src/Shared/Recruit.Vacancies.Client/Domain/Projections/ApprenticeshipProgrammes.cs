using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class ApprenticeshipProgrammes : IQueryProjection
    {
        public string Id { get; set; }
        public IEnumerable<ApprenticeshipProgramme> Programmes { get; set; } 
    }
}