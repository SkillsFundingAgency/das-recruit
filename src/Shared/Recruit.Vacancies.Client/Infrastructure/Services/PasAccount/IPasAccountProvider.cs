using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount
{
    public interface IPasAccountProvider
    {
        Task<bool> HasAgreementAsync(long ukprn);
    }
}