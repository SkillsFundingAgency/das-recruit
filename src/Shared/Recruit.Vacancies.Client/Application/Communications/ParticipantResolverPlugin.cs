using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications 
{
    public class ParticipantResolverPlugin : IParticipantResolver 
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILogger<ParticipantResolverPlugin> _logger;

        public string ParticipantResolverName => CommunicationConstants.ServiceName;

        public ParticipantResolverPlugin(
            IVacancyRepository vacancyRepository, 
            IUserRepository userRepository,
            ILogger<ParticipantResolverPlugin> logger) 
        {
            _userRepository = userRepository;
            _vacancyRepository = vacancyRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<CommunicationUser>> GetParticipantsAsync(CommunicationRequest request) 
        {
            _logger.LogDebug($"Resolving participants for RequestType: '{request.RequestType}'");
            var entityId = request.Entities.Single(e => e.EntityType == CommunicationConstants.EntityTypes.Vacancy).EntityId.ToString();
            long.TryParse(entityId, out var vacancyReference);
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
            return ConvertToCommunicationUser(users, vacancy.SubmittedByUser.UserId);
        }

        private IEnumerable<CommunicationUser> ConvertToCommunicationUser(List<User> user, string primaryUserId)
        {
            return user.Select(u => new CommunicationUser()
            {
                UserId = u.IdamsUserId,
                Email = u.Email,
                Name = u.Name,
                UserType = CommunicationConstants.UserType, 
                Participation = u.IdamsUserId == primaryUserId ? UserParticipation.PrimaryUser : UserParticipation.SecondaryUser
            });
        }
    }
}