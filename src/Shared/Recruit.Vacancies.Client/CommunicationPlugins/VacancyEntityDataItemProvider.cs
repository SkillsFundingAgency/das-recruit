using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.CommunicationPlugins
{
    public class VacancyEntityDataItemProvider : IEntityDataItemProvider
    {
        public string ProviderServiceName => CommunicationConstants.ServiceName;
        public string EntityType => CommunicationConstants.EntityTypes.Vacancy;

        public Task<IEnumerable<CommunicationDataItem>> GetDataItems(object entityId)
        {
            throw new System.NotImplementedException();
        }
    }
}