using Esfa.Recruit.Employer.Web.Models;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface ITrainingProviderService
    {
        Task<ProviderDetail> GetProviderDetailAsync(long ukprn);
        Task<bool> ExistsAsync(long ukprn);
    }
}
