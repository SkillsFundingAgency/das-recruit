using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Exceptions;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications
{
    public class VacancyEntityDataItemProviderPlugin : IEntityDataItemProvider
    {
        private readonly IVacancyRepository _vacancyRepository;
        public string EntityType => CommunicationConstants.EntityTypes.Vacancy;

        public VacancyEntityDataItemProviderPlugin(IVacancyRepository vacancyRepository)
        {
            _vacancyRepository = vacancyRepository;
        }

        public async Task<IEnumerable<CommunicationDataItem>> GetDataItems(object entityId)
        {
            if(int.TryParse(entityId.ToString(), out var vacancyReference) == false)
            {
                throw new InvalidEntityIdException(EntityType, nameof(VacancyEntityDataItemProviderPlugin));
            }

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);

            return new List<CommunicationDataItem>()
            {
                new CommunicationDataItem(VacancyDataItems.VacancyReference, vacancy.VacancyReference.ToString()),
                new CommunicationDataItem(VacancyDataItems.VacancyTitle, vacancy.Title),
                new CommunicationDataItem(VacancyDataItems.EmployerName, vacancy.EmployerName)
            };
        }
    }
}