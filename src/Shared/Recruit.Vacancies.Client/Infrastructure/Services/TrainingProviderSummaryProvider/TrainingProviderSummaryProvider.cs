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

        /// <summary>
        /// Method to check if the given ukprn number is a valid training provider with Main or Employer Profile with Status not equal to "Not Currently Starting New Apprentices".
        /// </summary>
        /// <param name="ukprn">ukprn number.</param>
        /// <returns>boolean.</returns>
        public async Task<bool> IsTrainingProviderMainOrEmployerProfile(long ukprn)
        {
            if (ukprn == EsfaTestTrainingProvider.Ukprn)
                return true;
            
            var provider = await _trainingProviderService.GetProviderDetails(ukprn);

            // logic to filter only Training provider with Main & Employer Profiles and Status Id not equal to "Not Currently Starting New Apprentices"
            return provider != null &&
                   (provider.ProviderTypeId.Equals((short) ProviderTypeIdentifier.MainProvider) ||
                    provider.ProviderTypeId.Equals((short) ProviderTypeIdentifier.EmployerProvider)) &&
                   !provider.StatusId.Equals((short) ProviderStatusType.ActiveButNotTakingOnApprentices);
        }
    }
}