using SFA.DAS.EAS.Account.Api.Types;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class StubEmployerAccountService : IEmployerAccountService
    {
        public Task<string[]> GetAccountIdentifiersAsync(string userId)
        {
            return Task.FromResult(new string[] { "abc", "xyz", "MYJR4X", "MB6YDY", "MXD78R" });
        }

        public Task<AccountDetailViewModel> GetAccountDetailAsync(string employerAccountId)
        {
            return Task.FromResult(new AccountDetailViewModel
            {
                DasAccountName = "Ozzy Scott"
            });
        }
    }
}