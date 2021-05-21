using System;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Application.Communications.ParticipantResolverPlugins;
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

            switch (requestType)
            {
                case CommunicationConstants.RequestType.VacancyRejected:
                    SetPreferencesForVacancyRejectedNotification(userPref, userPreference);
                    return userPref;
                case CommunicationConstants.RequestType.VacancyRejectedByEmployer:
                    SetPreferencesForVacancyRejectedByEmployerNotification(userPref, userPreference);
                    return userPref;
                case CommunicationConstants.RequestType.ApplicationSubmitted:
                    SetPreferencesForApplicationSubmittedNotification(userPref, userPreference);
                    return userPref;
                case CommunicationConstants.RequestType.VacancyWithdrawnByQa:
                case CommunicationConstants.RequestType.ProviderBlockedProviderNotification:
                case CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies:
                case CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies:
                case CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly:
                    SetPreferencesForMandatoryOrganisationEmailNotification(userPref);
                    return userPref;
                default:
                    throw new NotImplementedException($"User preferences not implemented for request {requestType}");
            }
        }

        private void SetPreferencesForVacancyRejectedNotification(CommunicationUserPreference userPref, UserNotificationPreferences userPreference)
        {
            if (userPreference == null) return;
            if (userPreference.NotificationTypes.HasFlag(NotificationTypes.VacancyRejected))
            {
                userPref.Channels = DeliveryChannelPreferences.EmailOnly;
                userPref.Frequency = DeliveryFrequency.Immediate;
                userPref.Scope = userPreference.NotificationScope.GetValueOrDefault().ConvertToCommunicationScope();
            }
        }

        private void SetPreferencesForVacancyRejectedByEmployerNotification(CommunicationUserPreference userPref, UserNotificationPreferences userPreference)
        {
            if (userPreference == null) return;
            if (userPreference.NotificationTypes.HasFlag(NotificationTypes.VacancyRejectedByEmployer))
            {
                userPref.Channels = DeliveryChannelPreferences.EmailOnly;
                userPref.Frequency = DeliveryFrequency.Immediate;
                userPref.Scope = userPreference.NotificationScope.GetValueOrDefault().ConvertToCommunicationScope();
            }
        }

        private void SetPreferencesForApplicationSubmittedNotification(CommunicationUserPreference userPref, UserNotificationPreferences userPreference)
        {
            if (userPreference == null) return;
            if (userPreference.NotificationTypes.HasFlag(NotificationTypes.ApplicationSubmitted))
            {
                userPref.Channels = DeliveryChannelPreferences.EmailOnly;
                userPref.Frequency = userPreference.NotificationFrequency.GetDeliveryFrequencyPreferenceFromUserFrequencyPreference();
                userPref.Scope = userPreference.NotificationScope.GetValueOrDefault().ConvertToCommunicationScope();
            }
        }

        private void SetPreferencesForMandatoryOrganisationEmailNotification(CommunicationUserPreference userPref)
        {
            userPref.Channels = DeliveryChannelPreferences.EmailOnly;
            userPref.Frequency = DeliveryFrequency.Immediate;
            userPref.Scope = NotificationScope.Organisation;
        }
    }
}