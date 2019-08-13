using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface ITrainingProviderSummaryProvider
    {
        Task<IEnumerable<TrainingProviderSummary>> FindAllAsync();
        Task<TrainingProviderSummary> GetAsync(long ukprn);
    }
}
