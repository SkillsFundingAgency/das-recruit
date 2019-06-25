using System.Collections.Generic;
using System.Threading.Tasks;

namespace Communication.Types.Interfaces
{
    public interface IEntityDataItemProvider
    {
        string ProviderServiceName { get; }
        /// EntityType will allow resolving data items required for the message template
        /// This should be a unique value across the services
        /// Example value: VacancyServices.Recruit.Vacancy
        string EntityType { get; }
        Task<IEnumerable<CommunicationDataItem>> GetDataItems(object entityId);
    }
}