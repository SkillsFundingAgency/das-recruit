using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Exceptions;
using Communication.Types.Interfaces;
using Esfa.Recruit.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins
{
    public class ApprenticeshipServiceUrlPlugin : IEntityDataItemProvider
    {
        private readonly CommunicationsConfiguration _communicationsConfiguration;
        private readonly IVacancyRepository _vacancyRepository;
        public string EntityType => CommunicationConstants.EntityTypes.ApprenticeshipServiceUrl;
        public ApprenticeshipServiceUrlPlugin(IVacancyRepository vacancyRepository, IOptions<CommunicationsConfiguration> communicationsConfiguration)
        {
            _vacancyRepository = vacancyRepository;
            _communicationsConfiguration = communicationsConfiguration.Value;
        }

        public async Task<IEnumerable<CommunicationDataItem>> GetDataItemsAsync(object entityId)
        {
            if(int.TryParse(entityId.ToString(), out var vacancyReference) == false)
            {
                throw new InvalidEntityIdException(EntityType, nameof(ApprenticeshipServiceUrlPlugin));
            }

            var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);

            var url = 
                vacancy.OwnerType == OwnerType.Employer 
                ? $"{_communicationsConfiguration.EmployersApprenticeshipServiceUrl}{vacancy.EmployerAccountId}" 
                : _communicationsConfiguration.ProvidersApprenticeshipServiceUrl;

            return new[] 
            { 
                new CommunicationDataItem(CommunicationConstants.DataItemKeys.ApprenticeshipService.ApprenticeshipServiceUrl, url)
            };
        }
    }
}