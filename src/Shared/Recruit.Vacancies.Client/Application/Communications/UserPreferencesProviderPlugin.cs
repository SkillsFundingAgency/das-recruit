using System;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using NotificationScope = Communication.Types.NotificationScope;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications;

public class UserPreferencesProviderPlugin : IUserPreferencesProvider
{
    public string UserType => CommunicationConstants.UserType;

    public async Task<CommunicationUserPreference> GetUserPreferenceAsync(string requestType, CommunicationUser user)
    {
        var userPref = new CommunicationUserPreference { Channels = DeliveryChannelPreferences.None };
        switch (requestType)
        {
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

    private void SetPreferencesForMandatoryOrganisationEmailNotification(CommunicationUserPreference userPref)
    {
        userPref.Channels = DeliveryChannelPreferences.EmailOnly;
        userPref.Frequency = DeliveryFrequency.Immediate;
        userPref.Scope = NotificationScope.Organisation;
    }
}