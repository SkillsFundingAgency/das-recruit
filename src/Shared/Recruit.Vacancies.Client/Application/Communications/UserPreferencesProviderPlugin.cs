using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public class UserPreferencesProviderPlugin : IUserPreferencesProvider
    {
        private readonly IUserNotificationPreferencesRepository _repository;
        public string UserType => CommunicationConstants.UserType;

        public UserPreferencesProviderPlugin(IUserNotificationPreferencesRepository repository)
        {
            _repository = repository;
        }

        public async Task<CommunicationUserPreference> GetUserPreference(string requestType, CommunicationUser user)
        {
            var userPref = new CommunicationUserPreference() { Channels = DeliveryChannelPreferences.None };

            var userPreference = await _repository.GetAsync(user.UserId);
            if (userPreference == null) return userPref;

            switch (requestType)
            {
                case CommunicationConstants.RequestType.VacancyRejected:
                    userPref = GetPreferencesForVacancyRejectedNotification(userPref, userPreference);
                    return userPref;
                default:
                    return userPref;
            }
        }

        private static CommunicationUserPreference GetPreferencesForVacancyRejectedNotification(CommunicationUserPreference userPref, UserNotificationPreferences userPreference)
        {
            if (userPreference.NotificationTypes.HasFlag(NotificationTypes.VacancyRejected))
            {
                userPref.Channels = DeliveryChannelPreferences.EmailOnly;
                userPref.Frequency = DeliveryFrequency.Immediate;
                userPref.Scope = userPreference.NotificationScope.GetValueOrDefault().ConvertToCommunicationScope();
            }

            return userPref;
        }
    }
}