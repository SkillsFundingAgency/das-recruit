using System;
using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
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
        
        public static string GetLastNonEmptyField(this Address address)
        {
            return new[]
            {
                address.AddressLine4,
                address.AddressLine3,
                address.AddressLine2,
                address.AddressLine1,
            }.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x));
        }

        public static IOrderedEnumerable<IGrouping<string, KeyValuePair<string, Address>>> GroupByLastFilledAddressLine(this List<Address> addresses)
        {
            return addresses?
                .Select(x => new KeyValuePair<string, Address>(GetLastNonEmptyField(x), x))
                .Where(x => !string.IsNullOrEmpty(x.Key))
                .GroupBy(x => x.Key, StringComparer.InvariantCultureIgnoreCase)
                .OrderBy(x => x.Key);
        }
        
        public static Address ToDomain(this GetAddressesListItem addressItem)
        {
            return addressItem is null
                ? null
                : new Address
                {
                    AddressLine1 = addressItem.AddressLine1,
                    AddressLine2 = addressItem.AddressLine2AndLine3,
                    AddressLine3 = addressItem.PostTown,
                    AddressLine4 = addressItem.County,
                    Postcode = addressItem.Postcode,
                    Country = addressItem.Country,
                };
        }
        
        public static string ToSingleLineFullAddress(this Address address)
        {
            string[] addressArray = [address.AddressLine1, address.AddressLine2, address.AddressLine3, address.AddressLine4, address.Postcode];
            return string.Join(", ", addressArray.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()));
        }
    
        public static string ToSingleLineAbridgedAddress(this Address address)
        {
            return $"{address.GetLastNonEmptyField()} ({address.Postcode})";
        }
    
        public static string ToSingleLineAnonymousAddress(this Address address)
        {
            return $"{address.GetLastNonEmptyField()} ({address.PostcodeAsOutcode()})";
        }
        
        public static IEnumerable<Address> OrderByCity(this IEnumerable<Address> addresses)
        {
            return addresses.OrderBy(x => x.GetLastNonEmptyField());
        }

        public static IEnumerable<string> GetPopulatedAddressLines(this Address address)
        {
            return new[]
            {
                address.AddressLine1,
                address.AddressLine2,
                address.AddressLine3,
                address.AddressLine4,
                address.Postcode
            }.Where(x => !string.IsNullOrEmpty(x?.Trim()));
        }
    }
}