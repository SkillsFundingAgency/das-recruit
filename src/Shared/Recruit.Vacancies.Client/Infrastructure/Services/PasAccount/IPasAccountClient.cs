using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.PasAccount
{
    public interface IPasAccountClient
    {
        Task<bool> HasAgreementAsync(long ukprn);
    }
}