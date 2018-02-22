using SFA.DAS.EAS.Account.Api.Types;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IGetAssociatedEmployerAccountsService
    {
        Task<string[]> GetAssociatedAccountsAsync(string userId);
        Task<AccountDetailViewModel> GetEmployerAccountAsync(string employerAccountId);
    }
}
