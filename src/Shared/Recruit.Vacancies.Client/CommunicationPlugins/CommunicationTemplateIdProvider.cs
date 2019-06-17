using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.CommunicationPlugins
{
    public class CommunicationTemplateIdProvider : ITemplateIdProvider
    {
        public string ProviderServiceName => CommunicationConstants.ServiceName;

        public Task<string> GetTemplateId(CommunicationMessage message)
        {
            throw new System.NotImplementedException();
        }
    }
}