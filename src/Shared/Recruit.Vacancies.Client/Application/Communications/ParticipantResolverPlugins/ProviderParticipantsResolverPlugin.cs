using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins
{
    public class ProviderParticipantsResolverPlugin : IParticipantResolver
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ProviderParticipantsResolverPlugin> _logger;
        public string ParticipantResolverName => CommunicationConstants.ParticipantResolverNames.ProviderParticipantsResolverName;

        public ProviderParticipantsResolverPlugin(IUserRepository userRepository, ILogger<ProviderParticipantsResolverPlugin> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request)
        {
            _logger.LogDebug($"Resolving participants for RequestType: '{request.RequestType}'");
            var entityId = request.Entities.Single(e => e.EntityType == CommunicationConstants.EntityTypes.Provider).EntityId.ToString();
            if(long.TryParse(entityId, out var ukprn) == false)
            {
                return Array.Empty<CommunicationUser>();
            }
            var users = await _userRepository.GetProviderUsers(ukprn);
            return ParticipantResolverPluginHelper.ConvertToCommunicationUsers(users, null);
        }
    }
}