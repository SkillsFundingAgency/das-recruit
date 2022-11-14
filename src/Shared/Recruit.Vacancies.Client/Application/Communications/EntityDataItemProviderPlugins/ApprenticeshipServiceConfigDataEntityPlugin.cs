using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;
using Esfa.Recruit.Client.Application.Communications;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins
{
    public class ApprenticeshipServiceConfigDataEntityPlugin : IEntityDataItemProvider
    {
        private readonly CommunicationsConfiguration _communicationsConfiguration;
        public string EntityType => CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig;
        public ApprenticeshipServiceConfigDataEntityPlugin(IOptions<CommunicationsConfiguration> communicationsConfiguration)
        {
            _communicationsConfiguration = communicationsConfiguration.Value;
        }

        public Task<IEnumerable<CommunicationDataItem>> GetDataItemsAsync(object entityId)
        {
            IEnumerable<CommunicationDataItem> dataItems = new [] { GetHelpdeskNumberDataItem() };
            return Task.FromResult(dataItems);
        }

        private CommunicationDataItem GetHelpdeskNumberDataItem()
        {
            return new CommunicationDataItem(CommunicationConstants.DataItemKeys.ApprenticeshipService.HelpdeskPhoneNumber, CommunicationConstants.HelpdeskPhoneNumber);
        }
    }
}