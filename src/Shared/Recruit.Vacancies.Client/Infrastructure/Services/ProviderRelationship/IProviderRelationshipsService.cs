using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    public interface IProviderRelationshipsService
    {
        Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn);
        Task<int> GetNumberOfPermittedLegalEntitesForEmployerAsync(long ukprn, string accountPublicHashedId);
        Task<bool> HasProviderGotEmployersPermissionAsync(long ukprn, string accountPublicHashedId, long legalEntityId);
        Task RevokeProviderPermissionToRecruitAsync(long ukprn, string accountLegalEntityPublicHashedId);
    }
}