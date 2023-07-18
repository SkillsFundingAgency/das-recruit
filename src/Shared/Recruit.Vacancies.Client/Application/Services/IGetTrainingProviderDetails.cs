using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IGetTrainingProviderDetails
    {
        Task<GetProviderResponse> GetTrainingProvider(long ukprn);
    }
}
