using SFA.DAS.EAS.Account.Api.Types;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class StubGetAssociatedEmployerAccountsService : IGetAssociatedEmployerAccountsService
    {
        public Task<string[]> GetAssociatedAccounts(string userId)
        {
            return Task.FromResult(new string[] { "abc", "xyz", "MYJR4X", "MB6YDY" });
        }

        public Task<AccountDetailViewModel> GetEmployerAccountAsync(string employerAccountId)
        {
            return Task.FromResult(new AccountDetailViewModel
            {
                DasAccountName = "Ozzy Scott"
            });
        }
    }
}