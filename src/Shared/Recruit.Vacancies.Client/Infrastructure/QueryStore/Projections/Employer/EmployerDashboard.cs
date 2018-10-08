using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer
{
    public class EmployerDashboard : QueryProjectionBase
    {
        public EmployerDashboard() : base(QueryViewType.EmployerDashboard.TypeName)
        {
        }

        public IEnumerable<VacancySummary> Vacancies { get; set; }

        public IEnumerable<VacancySummary> CloneableVacancies => Vacancies.Where(
            x => x.Status == VacancyStatus.Live ||
                 x.Status == VacancyStatus.Closed ||
                 x.Status == VacancyStatus.PendingReview);
    }
}
