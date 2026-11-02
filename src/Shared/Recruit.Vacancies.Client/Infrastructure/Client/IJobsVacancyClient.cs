using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IJobsVacancyClient
    {
        Task PatchTrainingProviderAsync(Guid vacancyId);
        Task<IEnumerable<LegalEntity>> GetEmployerLegalEntitiesAsync(string employerAccountId);
        Task CloseExpiredVacancies();
        Task RefreshEmployerProfiles(string employerAccountId, IEnumerable<string> accountLegalEntityPublicHashedIds);
        Task UpdateUserAccountAsync(string idamsUserId);
        Task UpdateProviders();
    }
}