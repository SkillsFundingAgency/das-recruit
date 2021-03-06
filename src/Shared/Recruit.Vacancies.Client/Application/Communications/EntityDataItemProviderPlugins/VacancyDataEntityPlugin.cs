using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Exceptions;
using Communication.Types.Interfaces;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using static Esfa.Recruit.Vacancies.Client.Application.Communications.CommunicationConstants;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins
{
    public class VacancyDataEntityPlugin : IEntityDataItemProvider
    {
        private readonly IVacancyRepository _vacancyRepository;
        public string EntityType => CommunicationConstants.EntityTypes.Vacancy;

        public VacancyDataEntityPlugin(IVacancyRepository vacancyRepository)
        {
            _vacancyRepository = vacancyRepository;
        }

        public async Task<IEnumerable<CommunicationDataItem>> GetDataItemsAsync(object entityId)
        {
            if(int.TryParse(entityId.ToString(), out var vacancyReference) == false)
            {
                throw new InvalidEntityIdException(EntityType, nameof(VacancyDataEntityPlugin));
            }

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);

            var employerName = vacancy.EmployerNameOption ==  EmployerNameOption.TradingName ? vacancy.EmployerName : vacancy.LegalEntityName;

            return new List<CommunicationDataItem>()
            {
                new CommunicationDataItem(DataItemKeys.Vacancy.VacancyReference, vacancy.VacancyReference.ToString()),
                new CommunicationDataItem(DataItemKeys.Vacancy.VacancyTitle, vacancy.Title),
                new CommunicationDataItem(DataItemKeys.Vacancy.EmployerName, employerName)
            };
        }
    }
}