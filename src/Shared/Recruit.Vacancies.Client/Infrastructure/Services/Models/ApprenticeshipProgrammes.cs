using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Models
{
    public class ApprenticeshipProgrammes : QueryProjectionBase
    {
        public IEnumerable<ApprenticeshipProgramme> Programmes { get; set; }
    }
}