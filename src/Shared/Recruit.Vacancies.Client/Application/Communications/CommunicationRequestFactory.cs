using Communication.Types;
using Humanizer;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public static class CommunicationRequestFactory
    {
        public static CommunicationRequest GetProviderBlockedEmployerNotificationForLiveVacanciesRequest(long ukprn, string employerAccountId)
        {
            var communicationRequest = new CommunicationRequest(
                    CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies, 
                    CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName, 
                    CommunicationConstants.ServiceName);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, ukprn);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Employer, employerAccountId);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig, null);
            return communicationRequest;
        }
        public static CommunicationRequest GetProviderBlockedEmployerNotificationForPermissionOnlyRequest(long ukprn, string employerAccountId)
        {
            var communicationRequest = new CommunicationRequest(
                    CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly, 
                    CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName, 
                    CommunicationConstants.ServiceName);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, ukprn);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Employer, employerAccountId);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig, null);
            return communicationRequest;
        }
        public static CommunicationRequest GetProviderBlockedEmployerNotificationForTransferredVacanciesRequest(long ukprn, string employerAccountId, int noOfVacancies)
        {
            var communicationRequest = new CommunicationRequest(
                    CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies, 
                    CommunicationConstants.ParticipantResolverNames.EmployerParticipantsResolverName, 
                    CommunicationConstants.ServiceName);
                var text = "advert".ToQuantity(noOfVacancies) + (noOfVacancies == 1 ? " has been transferred" : " have been transferred");
                communicationRequest.DataItems.Add(new CommunicationDataItem(CommunicationConstants.DataItemKeys.Employer.VacanciesTransferredCountText, text));
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Provider, ukprn);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.Employer, employerAccountId);
                communicationRequest.AddEntity(CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig, null);

            return communicationRequest;
        }
    }
}