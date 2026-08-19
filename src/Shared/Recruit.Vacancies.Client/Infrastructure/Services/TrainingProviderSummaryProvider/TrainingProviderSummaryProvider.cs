using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider
{
    /// <summary>
    /// Returns providers from RoATP (Register of Apprenticeship Training Providers)
    /// </summary>
    public class TrainingProviderSummaryProvider(ITrainingProviderService trainingProviderService)
        : ITrainingProviderSummaryProvider
    {
        public async Task<IEnumerable<TrainingProviderSummary>> FindAllAsync()
        {
            var response = await trainingProviderService.FindAllAsync();

            return response.Select(r => new TrainingProviderSummary
            {
                Ukprn = r.Ukprn.GetValueOrDefault(),
                ProviderName = r.Name
            });
        }

        public async Task<TrainingProviderSummary> GetAsync(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return new TrainingProviderSummary { Ukprn = EsfaTestTrainingProvider.Ukprn, ProviderName = EsfaTestTrainingProvider.Name, IsTrainingProviderMainOrEmployerProfile = true};

            var provider = await trainingProviderService.GetProviderDetails(ukprn);

            return new TrainingProviderSummary
            {
                Ukprn = provider.Ukprn,
                ProviderName = provider.Name,
                IsTrainingProviderMainOrEmployerProfile = (provider.ProviderTypeId.Equals((short)ProviderTypeIdentifier.MainProvider) ||
                                                           provider.ProviderTypeId.Equals((short)ProviderTypeIdentifier.EmployerProvider)) &&
                                                          !provider.StatusId.Equals((short)ProviderStatusType.ActiveButNotTakingOnApprentices)
            };
        }
    }
}