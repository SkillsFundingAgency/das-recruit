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
    }
}