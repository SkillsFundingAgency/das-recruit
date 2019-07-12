using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IBlockedOrganisationRepository
    {
        Task CreateAsync(BlockedOrganisation organisation);
        Task<BlockedOrganisation> GetAsync(Guid blockedOrganisationId);
    }
}
