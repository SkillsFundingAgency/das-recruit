using System.Threading;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Commands;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Events;
using Esfa.Recruit.Vacancies.Client.Domain.Messaging;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.CommandHandlers
{
    public class BlockProviderCommandHandler : IRequestHandler<BlockProviderCommand, Unit>
    {
        private readonly ILogger<BlockProviderCommandHandler> _logger;
        private readonly IBlockedOrganisationQuery _blockedOrganisationQuery;
        private readonly IBlockedOrganisationRepository _blockedOrganisationRepository;
        private readonly IMessaging _messaging;
        public BlockProviderCommandHandler(
            ILogger<BlockProviderCommandHandler> logger,
            IBlockedOrganisationQuery blockedOrganisationQuery,
            IBlockedOrganisationRepository blockedOrganisationRepository,
            IMessaging messaging)
        {
            _logger = logger;
            _blockedOrganisationQuery = blockedOrganisationQuery;
            _blockedOrganisationRepository = blockedOrganisationRepository;
            _messaging = messaging;
        }
        
        public async Task<Unit> Handle(BlockProviderCommand message, CancellationToken cancellationToken)
        {
            var blockedOrg = await _blockedOrganisationQuery.GetByOrganisationIdAsync(message.Ukprn.ToString());
            if (blockedOrg?.BlockedStatus == BlockedStatus.Blocked)
            {
                _logger.LogWarning($"Ignoring request to block provider with ukprn {message.Ukprn} as the provider is already blocked.");
                return Unit.Value;
            }

            await _blockedOrganisationRepository.CreateAsync(ConvertToBlockedOrganisation(message));

            await _messaging.PublishEvent(new ProviderBlockedEvent()
            {
                Ukprn = message.Ukprn,
                BlockedDate = message.BlockedDate,
                QaVacancyUser = message.QaVacancyUser
            });
            return Unit.Value;
        }

        private static BlockedOrganisation ConvertToBlockedOrganisation(BlockProviderCommand message)
        {
            return new BlockedOrganisation()
            {
                BlockedStatus = BlockedStatus.Blocked,
                OrganisationType = OrganisationType.Provider,
                OrganisationId = message.Ukprn.ToString(),
                UpdatedByUser = message.QaVacancyUser,
                UpdatedDate = message.BlockedDate,
                Reason = message.Reason
            };
        }
    }
}