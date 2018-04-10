using Esfa.Recruit.Employer.Web.Models;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface ITrainingProviderService
    {
        Task<TrainingProvider> GetProviderAsync(long ukprn);
        Task<bool> ExistsAsync(long ukprn);
    }
}
