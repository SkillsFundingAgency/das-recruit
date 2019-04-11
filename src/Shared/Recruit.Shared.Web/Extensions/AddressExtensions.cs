using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Projections = Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Recruit.Shared.Web.Extensions
{
    public static class AddressExtensions
    {
        public static string ToAddressString(this IAddress address)
        {
            var addressArray = new[] 
            {
                address.AddressLine1.Trim(), 
                address.AddressLine2.Trim(), 
                address.AddressLine3.Trim(), 
                address.AddressLine4.Trim(), 
                address.Postcode.Trim() 
            };
            return string
                .Join(", ", addressArray)
                .Replace(" ,", string.Empty);
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
    }
}