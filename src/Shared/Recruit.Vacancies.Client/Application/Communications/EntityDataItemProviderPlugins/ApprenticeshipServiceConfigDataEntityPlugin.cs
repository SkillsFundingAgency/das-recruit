using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins
{
    public class ApprenticeshipServiceConfigDataEntityPlugin : IEntityDataItemProvider
    {
        public string EntityType => CommunicationConstants.EntityTypes.ApprenticeshipServiceConfig;
        
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