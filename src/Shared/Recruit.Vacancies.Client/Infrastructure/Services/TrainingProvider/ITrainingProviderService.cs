using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public interface ITrainingProviderService
    {
        Task<Domain.Entities.TrainingProvider> GetProviderAsync(long ukprn);
        Task<IEnumerable<Domain.Entities.TrainingProvider>> FindAllAsync();
    }
}
