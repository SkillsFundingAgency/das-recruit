using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Cache;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProviderSummaryProvider
{
    /// <summary>
    /// Returns providers from RoATP (Register of Apprenticeship Training Providers)
    /// </summary>
    public class TrainingProviderSummaryProvider : ITrainingProviderSummaryProvider
    {
        private readonly ITrainingProviderService _trainingProviderService;
        
        public TrainingProviderSummaryProvider(ITrainingProviderService trainingProviderService)
        {
            _trainingProviderService = trainingProviderService;
        }

        public async Task<IEnumerable<TrainingProviderSummary>> FindAllAsync()
        {
            var response = await _trainingProviderService.FindAllAsync();

            return response.Select(r => new TrainingProviderSummary
            {
                Ukprn = r.Ukprn.Value,
                ProviderName = r.Name
            });
        }

        public async Task<TrainingProviderSummary> GetAsync(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return new TrainingProviderSummary { Ukprn = EsfaTestTrainingProvider.Ukprn, ProviderName = EsfaTestTrainingProvider.Name };

            var provider = await _trainingProviderService.GetProviderAsync(ukprn);
            
            return new TrainingProviderSummary
            {
                Ukprn = provider.Ukprn.Value,
                ProviderName = provider.Name
            };
        }

        
    }
}