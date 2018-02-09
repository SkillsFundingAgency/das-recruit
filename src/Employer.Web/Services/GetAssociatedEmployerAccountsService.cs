namespace Employer.Web.Services
{
    public class GetAssociatedEmployerAccountsService : IGetAssociatedEmployerAccountsService
    {
        public string[] GetAssociatedAccounts(string userId)
        {
            return new string[] { "abc", "xyz" };
        }
    }
}
