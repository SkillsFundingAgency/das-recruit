using Esfa.Recruit.Vacancies.Client.Domain.Projections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.QueryStore
{
    public interface IQueryStoreWriter
    {
        Task UpdateDashboardAsync(string employerAccountId, IEnumerable<VacancySummary> vacancySummaries); 

        Task UpdateApprenticeshipProgrammesAsync(IEnumerable<ApprenticeshipProgramme> programmes);
    }
}