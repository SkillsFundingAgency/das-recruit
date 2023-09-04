using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface ITrainingProviderSummaryProvider
    {
        Task<IEnumerable<TrainingProviderSummary>> FindAllAsync();
        Task<TrainingProviderSummary> GetAsync(long ukprn);
        /// <summary>
        /// Contract to check if the given ukprn is a valid training provider.
        /// </summary>
        /// <param name="ukprn">ukprn number.</param>
        /// <returns>boolean.</returns>
        Task<bool> IsTrainingProviderMainOrEmployerProfile(long ukprn);
    }
}
