using SFA.DAS.EAS.Account.Api.Types;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IEmployerAccountService
    {
        Task<string[]> GetAccountIdentifiersAsync(string userId);
        Task<AccountDetailViewModel> GetAccountDetailAsync(string employerAccountId);
    }
}
