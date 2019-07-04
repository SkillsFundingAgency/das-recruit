using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Application.CommunicationPlugins
{
    public class CommunicationTemplateIdProvider : ITemplateIdProvider
    {
        public string ProviderServiceName => CommunicationConstants.ServiceName;

        public Task<string> GetTemplateId(CommunicationMessage message)
        {
            var templateId = string.Empty;
            switch(message.RequestType)
            {
                case CommunicationConstants.RequestType.VacancyRejected:
                    templateId =  $"RecruitV2_{CommunicationConstants.RequestType.VacancyRejected}";
                    break;
                default:
                    break;
            }
            return Task.FromResult(templateId);
        }
    }
}