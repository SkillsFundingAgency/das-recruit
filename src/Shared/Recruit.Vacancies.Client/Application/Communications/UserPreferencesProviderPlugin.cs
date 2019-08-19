using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using NotificationScope = Communication.Types.NotificationScope;

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

        public async Task<CommunicationUserPreference> GetUserPreferenceAsync(string requestType, CommunicationUser user)
        {
            var userPref = new CommunicationUserPreference() { Channels = DeliveryChannelPreferences.None };

            var userPreference = await _repository.GetAsync(user.UserId);
            if (userPreference == null) return userPref;

            switch (requestType)
            {
                case CommunicationConstants.RequestType.VacancyRejected:
                    SetPreferencesForVacancyRejectedNotification(ref userPref, userPreference);
                    return userPref;
                case CommunicationConstants.RequestType.ApplicationSubmitted:
                    SetPreferencesForApplicationSubmittedNotification(ref userPref, userPreference);
                    return userPref;
                case CommunicationConstants.RequestType.VacancyWithdrawnByQa:
                    SetPreferencesForMandatoryOrganisationEmailNotification(ref userPref);
                    return userPref;
                default:
                    return userPref;
            }
        }

        private static void SetPreferencesForVacancyRejectedNotification(ref CommunicationUserPreference userPref, UserNotificationPreferences userPreference)
        {
            if (userPreference.NotificationTypes.HasFlag(NotificationTypes.VacancyRejected))
            {
                userPref.Channels = DeliveryChannelPreferences.EmailOnly;
                userPref.Frequency = DeliveryFrequency.Immediate;
                userPref.Scope = userPreference.NotificationScope.GetValueOrDefault().ConvertToCommunicationScope();
            }
        }

        private static void SetPreferencesForApplicationSubmittedNotification(ref CommunicationUserPreference userPref, UserNotificationPreferences userPreference)
        {
            if (userPreference.NotificationTypes.HasFlag(NotificationTypes.ApplicationSubmitted))
            {
                userPref.Channels = DeliveryChannelPreferences.EmailOnly;
                userPref.Frequency = DeliveryFrequency.Immediate;
                userPref.Scope = userPreference.NotificationScope.GetValueOrDefault().ConvertToCommunicationScope();
            }
        }

        private static void SetPreferencesForMandatoryOrganisationEmailNotification(ref CommunicationUserPreference userPref)
        {
            userPref.Channels = DeliveryChannelPreferences.EmailOnly;
            userPref.Frequency = DeliveryFrequency.Immediate;
            userPref.Scope = NotificationScope.Organisation;
        }
    }
}