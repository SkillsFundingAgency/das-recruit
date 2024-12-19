using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Projections = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class AddressExtensions
    {
        public static string ToAddressString(this IAddress address)
        {
            var addressArray = new[] 
            {
                address.AddressLine1, 
                address.AddressLine2, 
                address.AddressLine3, 
                address.AddressLine4, 
                address.Postcode 
            };
            return string
                .Join(", ", addressArray.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()));
        }

        public static Address ConvertToDomainAddress(this Projections.EditVacancyInfo.Address address)
        {
            return new Address() {
                AddressLine1 = address.AddressLine1,
                AddressLine2 = address.AddressLine2,
                AddressLine3 = address.AddressLine3,
                AddressLine4 = address.AddressLine4,
                Postcode = address.Postcode
            };
        }
        
        public static bool IsEmpty(this Address address)
        {
            return new[]
            {
                address.AddressLine1,
                address.AddressLine2,
                address.AddressLine3,
                address.AddressLine4,
                address.Postcode
            }.All(x => string.IsNullOrEmpty(x?.Trim()));
        }

        public static IOrderedEnumerable<IGrouping<string, KeyValuePair<string, Address>>> GroupByLastFilledAddressLine(this List<Address> addresses)
        {
            return addresses?
                .Select(x => new KeyValuePair<string, Address>(Selector(x), x))
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .GroupBy(x => x.Key)
                .OrderBy(x => x.Key);

            string Selector(Address address) => new[]
                {
                    address.AddressLine4,
                    address.AddressLine3,
                    address.AddressLine2,
                    address.AddressLine1,
                }.FirstOrDefault(x => !string.IsNullOrEmpty(x?.Trim()));
        }
        
        public static Address ToDomain(this GetAddressesListItem addressItem)
        {
            return new Address
            {
                AddressLine1 = addressItem?.AddressLine1,
                AddressLine2 = addressItem?.AddressLine2AndLine3,
                AddressLine3 = addressItem?.PostTown,
                AddressLine4 = addressItem?.County,
                Postcode = addressItem?.Postcode
            };
        }
    }
}