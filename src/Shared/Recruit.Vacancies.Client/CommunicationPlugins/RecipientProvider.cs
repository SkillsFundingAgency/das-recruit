using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.CommunicationPlugins 
{
    public class RecipientResolver : IParticipantResolver 
    {
        private readonly IVacancyRepository _vacancyRepository;
        private readonly IUserRepository _userRepository;
        public RecipientResolver (IVacancyRepository vacancyRepository, IUserRepository userRepository) 
        {
            _userRepository = userRepository;
            _vacancyRepository = vacancyRepository;
        }
        public string ResolverServiceName => CommunicationConstants.ServiceName;

        public async Task<IEnumerable<CommunicationUser>> GetRecipientsAsync (CommunicationRequest request) 
        {
            var entityId = request.Entities.FirstOrDefault (e => e.EntityType == CommunicationConstants.EntityTypes.Vacancy).EntityId.ToString ();
            long.TryParse (entityId, out var vacancyReference);
            var vacancy = await _vacancyRepository.GetVacancyAsync (vacancyReference);
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
                Email = u.Email,
                Name = u.Name,
                UserType = "VacancyServices.Recruit.User", // ??? 
                Participation = u.IdamsUserId == primaryUserId ? UserParticipation.PrimaryUser : UserParticipation.SecondaryUser
            });
        }
    }
}