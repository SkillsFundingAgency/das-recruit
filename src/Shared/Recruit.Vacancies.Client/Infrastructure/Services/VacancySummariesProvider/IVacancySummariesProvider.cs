using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    public interface IVacancySummariesProvider
    {
         Task<IList<VacancySummary>> GetEmployerOwnedVacancySummariesByEmployerAccountAsync(string employerAccountId);
         Task<IList<VacancySummary>> GetProviderOwnedVacancySummariesByUkprnAsync(long ukprn);
         Task<IList<TransferInfo>> GetTransferredFromProviderAsync(long ukprn);
    }
}