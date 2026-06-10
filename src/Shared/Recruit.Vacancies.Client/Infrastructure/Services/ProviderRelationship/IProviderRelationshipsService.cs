using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship
{
    public interface IProviderRelationshipsService
    {
        Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProviderAsync(long ukprn, List<OperationType> operationTypes);
        Task<IEnumerable<EmployerInfo>> GetLegalEntitiesForProvider(long ukprn, string accountHashedId, List<OperationType> operationTypes);
        Task<bool> HasProviderGotEmployersPermissionAsync(long ukprn, string accountPublicHashedId, string accountLegalEntityPublicHashedId, OperationType operationType);
        Task<bool> CheckEmployerHasPermissions(string accountHashedId, OperationType operationType);
    }
}