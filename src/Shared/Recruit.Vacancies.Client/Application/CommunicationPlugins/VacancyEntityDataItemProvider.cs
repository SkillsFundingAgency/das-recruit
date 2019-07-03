using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Exceptions;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Application.Services;

namespace Esfa.Recruit.Vacancies.Client.Application.CommunicationPlugins
{
    public class VacancyEntityDataItemProvider : IEntityDataItemProvider
    {
        public string EntityType => CommunicationConstants.EntityTypes.Vacancy;

        public VacancyEntityDataItemProvider(IVacancyService vacancyService)
        {
            
        }

        public async Task<IEnumerable<CommunicationDataItem>> GetDataItems(object entityId)
        {
            if(int.TryParse(entityId.ToString(), out var vacancyReference) == false)
            {
                throw new InvalidEntityIdException(EntityType, nameof(VacancyEntityDataItemProvider));
            }

            return await Task.FromResult(new List<CommunicationDataItem>());
        }
    }
}