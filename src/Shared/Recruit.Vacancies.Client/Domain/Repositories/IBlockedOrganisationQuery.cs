using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IBlockedOrganisationQuery
    {
        Task<List<BlockedOrganisation>> GetByOrganisationIdAsync(string organisationId);
    }
}