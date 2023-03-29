using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

namespace Esfa.Recruit.Employer.Web.Models
{
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
}
