using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System.Linq;

namespace Esfa.Recruit.Employer.Web.Extensions
{
    public static class AddressExtensions
    {
        public static string GetInlineAddress(this Address addr)
        {
            var arr = new string[]
               {
                addr.AddressLine1,
                addr.AddressLine2,
                addr.AddressLine3,
                addr.AddressLine4,
                addr.Postcode
               };

            return string.Join(", ", arr.Where(x => !string.IsNullOrWhiteSpace(x)));
        }
    }
}
