using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins
{
    public class EmployerDataEntityPlugin : IEntityDataItemProvider
    {
        public string EntityType  => CommunicationConstants.EntityTypes.Employer;

        public Task<IEnumerable<CommunicationDataItem>> GetDataItemsAsync(object entityId)
        {
            return Task.FromResult(Enumerable.Empty<CommunicationDataItem>());
        }
    }
}