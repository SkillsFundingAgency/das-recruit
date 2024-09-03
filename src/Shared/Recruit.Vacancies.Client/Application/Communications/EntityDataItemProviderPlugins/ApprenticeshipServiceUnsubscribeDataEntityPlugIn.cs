using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Communication.Types;
using Communication.Types.Exceptions;
using Communication.Types.Interfaces;
using Esfa.Recruit.Client.Application.Communications;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Application.Communications.EntityDataItemProviderPlugins;

public class ApprenticeshipServiceUnsubscribeDataEntityPlugIn : IEntityDataItemProvider
{
    private readonly CommunicationsConfiguration _communicationsConfiguration;
    private readonly IVacancyRepository _vacancyRepository;
    public string EntityType => CommunicationConstants.EntityTypes.ApprenticeshipServiceUnsubscribeUrl;
    public ApprenticeshipServiceUnsubscribeDataEntityPlugIn(IVacancyRepository vacancyRepository, IOptions<CommunicationsConfiguration> communicationsConfiguration)
    {
        _vacancyRepository = vacancyRepository;
        _communicationsConfiguration = communicationsConfiguration.Value;
    }

    public async Task<IEnumerable<CommunicationDataItem>> GetDataItemsAsync(object entityId)
    {
        if (long.TryParse(entityId.ToString(), out var vacancyReference) == false)
        {
            throw new InvalidEntityIdException(EntityType, nameof(ApprenticeshipServiceUnsubscribeDataEntityPlugIn));
        }

        var vacancy = await _vacancyRepository.GetVacancyAsync(vacancyReference);

            
        return new [] { GetApplicationUrlDataItem(vacancy) };
    }

    private CommunicationDataItem GetApplicationUrlDataItem(Vacancy vacancy)
    {
        var url = string.Empty;
        if (vacancy.OwnerType == OwnerType.Employer || vacancy.Status == VacancyStatus.Review)
        {
            var baseUri = new Uri(_communicationsConfiguration.EmployersApprenticeshipServiceUrl);
            var uri = new Uri(baseUri, $"{vacancy.EmployerAccountId}/notifications-manage");
            url = uri.ToString();
        }
        else
        {
            var baseUri = new Uri(_communicationsConfiguration.ProvidersApprenticeshipServiceUrl);
            var uri = new Uri(baseUri, $"{vacancy.TrainingProvider.Ukprn}/notifications-manage");
            url = uri.ToString();
        }

        return new CommunicationDataItem(CommunicationConstants.DataItemKeys.ApprenticeshipService.ApprenticeshipServiceUnsubscribeUrl, url);
    }
}