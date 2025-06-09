using System.Collections.Generic;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Extensions
{
    public static class AddressExtensions
    {
        private const int PostcodeMinLength = 5;
        private const int IncodeLength = 3;

        public static string PostcodeAsOutcode(this Address address)
        {
            var postcode = address.Postcode.Replace(" ", "");

            if (postcode.Length < PostcodeMinLength)
            {
                return null;
            }

            return postcode.Substring(0, postcode.Length - IncodeLength);
        }

        public static string Flatten(this Address address)
        {
            return new[]
            {
                address.AddressLine1,
                address.AddressLine2,
                address.AddressLine3,
                address.AddressLine4,
                address.Postcode
            }.Where(x => !string.IsNullOrWhiteSpace(x)).ToDelimitedString(", ");
        }

        public static string GetCity(this Address? address)
        {
            if (address is null)
            {
                return null;
            }

            // city should never be on first line
            List<string> lines = [
                address.AddressLine4,
                address.AddressLine3,
                address.AddressLine2
            ];

            return lines.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))?.Trim();
        }

        public static string GetCities(this IEnumerable<Address> addresses)
        {
            // Group by city
            var cityGroups = addresses
                .Where(a => !string.IsNullOrWhiteSpace(a.GetCity()))
                .GroupBy(a => a.GetCity());

            // Build display values
            var displayValues = cityGroups.SelectMany(group =>
            {
                bool isDuplicate = group.Count() > 1;
                return group.Select(a =>
                    isDuplicate
                        ? $"{a.GetCity()} ({a.AddressLine1?.Trim()})"
                        : a.GetCity().Trim()
                );
            });

            return string.Join(", ", displayValues);
        }
    }
}
