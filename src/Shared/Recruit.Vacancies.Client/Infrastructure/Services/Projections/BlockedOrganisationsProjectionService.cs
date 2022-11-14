using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.BlockedOrganisations;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Projections
{
    public class BlockedOrganisationsProjectionService : IBlockedOrganisationsProjectionService
    {
        private readonly ILogger<BlockedOrganisationsProjectionService> _logger;
        private readonly IBlockedOrganisationQuery _repo;
        private readonly IQueryStoreWriter _queryStoreWriter;

        public BlockedOrganisationsProjectionService(ILogger<BlockedOrganisationsProjectionService> logger, IBlockedOrganisationQuery repo, IQueryStoreWriter queryStoreWriter)
        {
            _logger = logger;
            _repo = repo;
            _queryStoreWriter = queryStoreWriter;
        }

        public async Task RebuildBlockedProviderOrganisations()
        {
            var blockedProviders = await _repo.GetAllBlockedProvidersAsync();

            _logger.LogInformation($"Found {blockedProviders.Count} currently blocked providers to populate queryStore {nameof(BlockedProviderOrganisations)} document.");
            await _queryStoreWriter.UpdateBlockedProviders(blockedProviders);
        }

        public async Task RebuildBlockedEmployerOrganisations()
        {
            var blockedEmployers = await _repo.GetAllBlockedEmployersAsync();

            _logger.LogInformation($"Found {blockedEmployers.Count} currently blocked employers to populate queryStore {nameof(BlockedEmployerOrganisations)} document.");

            await _queryStoreWriter.UpdateBlockedEmployers(blockedEmployers);
        }

        public Task RebuildAllBlockedOrganisationsAsync()
        {
            return Task.WhenAll(RebuildBlockedProviderOrganisations(),RebuildBlockedEmployerOrganisations());
        }
    }
}