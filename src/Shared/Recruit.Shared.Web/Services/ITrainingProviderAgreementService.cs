using System.Threading.Tasks;

namespace Esfa.Recruit.Shared.Web.Services
{
    public interface ITrainingProviderAgreementService
    {
        Task<bool> HasAgreementAsync(long ukprn);
    }
}