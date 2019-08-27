using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins
{
    public class VacancyParticipantsResolverPlugin : IParticipantResolver 
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<VacancyParticipantsResolverPlugin> _logger;

        public string ParticipantResolverName => CommunicationConstants.ParticipantResolverNames.VacancyParticipantsResolverName;

        public VacancyParticipantsResolverPlugin(
            IVacancyRepository vacancyRepository, 
            IUserRepository userRepository,
            ILogger<VacancyParticipantsResolverPlugin> logger) 
        {
            _userRepository = userRepository;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request) 
        {
            _logger.LogDebug($"Resolving participants for RequestType: '{request.RequestType}'");
            
            var entityId = request.Entities.Single(e => e.EntityType == CommunicationConstants.EntityTypes.Vacancy).EntityId.ToString();
            if(long.TryParse(entityId, out var vacancyReference) == false)
            {
                return Array.Empty<CommunicationUser>();
            }

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);
            List<User> users = null;
            if (vacancy.OwnerType == OwnerType.Employer) 
            {
                users = await _userRepository.GetEmployerUsersAsync(vacancy.EmployerAccountId);
            }
            else
            {
                users = await _userRepository.GetProviderUsers(vacancy.TrainingProvider.Ukprn.GetValueOrDefault());
            }
            return ParticipantResolverPluginHelper.ConvertToCommunicationUser(users, vacancy.SubmittedByUser.UserId);
        }
    }
}