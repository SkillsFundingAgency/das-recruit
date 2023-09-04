using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.TrainingProvider
{
    public interface ITrainingProviderService
    {
        Task<Domain.Entities.TrainingProvider> GetProviderAsync(long ukprn);
        Task<IEnumerable<Domain.Entities.TrainingProvider>> FindAllAsync();

        /// <summary>
        /// Contract to get the details of Provider from Outer API by given ukprn number.
        /// </summary>
        /// <param name="ukprn">ukprn number.</param>
        /// <returns></returns>
        Task<GetProviderResponseItem> GetProviderDetails(long ukprn);
    }
}
