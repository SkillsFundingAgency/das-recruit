using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Vacancy;

namespace SFA.DAS.Recruit.Api.Models
{
    public class ClosedVacanciesSummary
    {
        public IEnumerable<ClosedVacancy> Vacancies { get; set; }
    }
}
