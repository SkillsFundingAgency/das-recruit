using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.QueryStore
{
    public interface IQueryStoreWriter
    {
        Task UpdateDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries); 

        Task UpdateApprenticeshipProgrammesAsync(ApprenticeshipProgrammes programmes);
    }
}