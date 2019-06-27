using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Providers
{
    public interface ITrainingProviderAgreementProvider
    {
        Task<bool> HasAgreementAsync(long ukprn);
    }
}