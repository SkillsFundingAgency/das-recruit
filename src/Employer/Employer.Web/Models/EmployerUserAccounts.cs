using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Models
{
    public class EmployerUserAccounts
    {
        public string Email { get; set; }
        public string EmployerUserId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public IEnumerable<EmployerUserAccountItem> EmployerAccounts { get; set; }

        public static implicit operator EmployerUserAccounts(GetUserAccountsResponse source)
        {
            if (source?.UserAccounts == null)
            {
                return new EmployerUserAccounts
                {
                    FirstName = source?.FirstName,
                    LastName = source?.LastName,
                    EmployerUserId = source?.EmployerUserId,
                    EmployerAccounts = new List<EmployerUserAccountItem>(),
                };
            }

            return new EmployerUserAccounts
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                EmployerUserId = source.EmployerUserId,
                EmployerAccounts = source.UserAccounts.Select(c => (EmployerUserAccountItem)c).ToList(),
            };
        }
    }
}

public class EmployerUserAccountItem
{
    public string EncodedAccountId { get; set; }
    public string DasAccountName { get; set; }
    public string Role { get; set; }

    public static implicit operator EmployerUserAccountItem(EmployerIdentifier source)
    {
        return new EmployerUserAccountItem
        {
            EncodedAccountId = source.AccountId,
            DasAccountName = source.EmployerName,
            Role = source.Role
        };
    }
}
