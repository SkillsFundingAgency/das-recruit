using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider
{
    public class ProviderDashboard : QueryProjectionBase
    {
        public ProviderDashboard() : base(QueryViewType.ProviderDashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummary> Vacancies { get; set; }

        public IEnumerable<VacancySummary> CloneableVacancies => Vacancies.Where(
            x => x.Status == VacancyStatus.Live ||
                 x.Status == VacancyStatus.Closed ||
                 x.Status == VacancyStatus.Submitted);
    }
}
