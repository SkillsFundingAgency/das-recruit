using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public class TemplateIdProviderPlugin : ITemplateIdProvider
    {
        public string ProviderServiceName => CommunicationConstants.ServiceName;

        public Task<string> GetTemplateId(CommunicationMessage message)
        {
            var templateId = string.Empty;
            switch(message.RequestType)
            {
                case CommunicationConstants.RequestType.VacancyRejected:
                    templateId =  CommunicationConstants.TemplateIds.VacancyRejected;
                    break;
                case CommunicationConstants.RequestType.ApplicationSubmitted:
                    if (message.Frequency == DeliveryFrequency.Immediate)
                        templateId = CommunicationConstants.TemplateIds.ApplicationSubmitted;
                    break;
                default:
                    break;
            }
            return Task.FromResult(templateId);
        }
    }
}