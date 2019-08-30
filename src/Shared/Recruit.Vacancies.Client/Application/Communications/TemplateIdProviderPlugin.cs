using System;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public class TemplateIdProviderPlugin : ITemplateIdProvider
    {
        public string ProviderServiceName => CommunicationConstants.ServiceName;

        public Task<string> GetTemplateIdAsync(CommunicationMessage message)
        {
            var templateId = string.Empty;
            switch(message.RequestType)
            {
                case CommunicationConstants.RequestType.VacancyRejected:
                    templateId = CommunicationConstants.TemplateIds.VacancyRejected;
                    break;
                case CommunicationConstants.RequestType.ApplicationSubmitted:
                    templateId = CommunicationConstants.TemplateIds.ApplicationSubmittedImmediate;
                    break;
                case CommunicationConstants.RequestType.VacancyWithdrawnByQa:
                    templateId = CommunicationConstants.TemplateIds.VacancyWithdrawnByQa;
                    break;
                case CommunicationConstants.RequestType.ProviderBlockedProviderNotification:
                    templateId = CommunicationConstants.TemplateIds.ProviderBlockedProviderNotification;
                    break;
                case CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForTransferredVacancies:
                    templateId = CommunicationConstants.TemplateIds.ProviderBlockedEmployerNotificationForTransferredVacancies;
                    break;
                case CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForLiveVacancies:
                    templateId = CommunicationConstants.TemplateIds.ProviderBlockedEmployerNotificationForLiveVacancies;
                    break;
                case CommunicationConstants.RequestType.ProviderBlockedEmployerNotificationForPermissionOnly:
                    templateId = CommunicationConstants.TemplateIds.ProviderBlockedEmployerNotificationForPermissionsOnly;
                    break;
                default:
                    throw new NotImplementedException($"Template for request type {message.RequestType} is not defined.");
            }
            return Task.FromResult(templateId);
        }
    }
}