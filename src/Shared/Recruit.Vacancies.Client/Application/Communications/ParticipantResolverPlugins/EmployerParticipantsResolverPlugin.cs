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
    public class EmployerParticipantsResolverPlugin : IParticipantResolver
    {
        public string ParticipantResolverName => CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName;

        private readonly IUserRepository _userRepository;
        private readonly ILogger<EmployerParticipantsResolverPlugin> _logger;
        public EmployerParticipantsResolverPlugin(IUserRepository userRepository, ILogger<EmployerParticipantsResolverPlugin> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request)
        {
            _logger.LogInformation($"Resolving participants for RequestType: '{request.RequestType}'");
            var employerAccountId = request.Entities.Single(e => e.EntityType == CommunicationConstants.EntityTypes.Employer).EntityId.ToString();
            if(string.IsNullOrWhiteSpace(employerAccountId))
            {
                return Array.Empty<CommunicationUser>();
            }
            var users = await _userRepository.GetEmployerUsersAsync(employerAccountId);
            return ParticipantResolverPluginHelper.ConvertToCommunicationUsers(users, null);
        }

        public Task<IEnumerable<CommunicationMessage>> ValidateParticipantAsync(IEnumerable<CommunicationMessage> messages)
        {
            throw new NotImplementedException();
        }
    }
}