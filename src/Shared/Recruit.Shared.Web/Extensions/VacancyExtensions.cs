using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.Extensions
{
    public static class VacancyExtensions
    {
        private static IEnumerable<string> MapAddress(Address address)
        {
            return new[]
                {
                    address.AddressLine1,
                    address.AddressLine2,
                    address.AddressLine3,
                    address.AddressLine4,
                    address.Postcode
                }
                .Where(x => !string.IsNullOrEmpty(x));
        }
        
        public static IEnumerable<string> EmployerAddressForDisplay(this Vacancy vacancy)
        {
            if (vacancy.EmployerLocation == null)
            {
                return [];
            }

            return vacancy.IsAnonymous 
                ? [vacancy.EmployerLocation.PostcodeAsOutcode()]
                : MapAddress(vacancy.EmployerLocation);
        }
        
        public static IEnumerable<IEnumerable<string>> EmployerAddressesForDisplay(this Vacancy vacancy)
        {
            if (vacancy.EmployerLocations is null)
            {
                return [];
            }

            return vacancy.IsAnonymous
                ? vacancy.EmployerLocations.Select(address => new[] { address.PostcodeAsOutcode() })
                : vacancy.EmployerLocations.Select(MapAddress);
        }
    }
}
