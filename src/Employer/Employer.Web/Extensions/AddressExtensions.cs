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

        public static Address ConvertToDomainAddress(Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo.Address address)
        {
            return new Address() {
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                AddressLine4 = address.AddressLine4,
                Postcode = address.Postcode
            };
        }
    }
}
