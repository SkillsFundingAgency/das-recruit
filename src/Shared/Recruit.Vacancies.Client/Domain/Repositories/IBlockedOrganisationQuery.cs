using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IBlockedOrganisationQuery
    {
        Task<BlockedOrganisation> GetByOrganisationIdAsync(string organisationId);
        Task<List<BlockedOrganisationSummary>> GetAllBlockedProvidersAsync();
        Task<List<BlockedOrganisationSummary>> GetAllBlockedEmployersAsync();
    }
}