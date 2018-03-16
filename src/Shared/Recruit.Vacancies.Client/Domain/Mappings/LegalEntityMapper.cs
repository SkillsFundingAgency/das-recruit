using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.EAS.Account.Api.Types;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Esfa.Recruit.Vacancies.Client.Domain.Mappings
{
    public static class LegalEntityMapper
    {
        internal static Regex PostcodeRegex => new Regex(@"(([gG][iI][rR] {0,}0[aA]{2})|((([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y]?[0-9][0-9]?)|(([a-pr-uwyzA-PR-UWYZ][0-9][a-hjkstuwA-HJKSTUW])|([a-pr-uwyzA-PR-UWYZ][a-hk-yA-HK-Y][0-9][abehmnprv-yABEHMNPRV-Y]))) {0,}[0-9][abd-hjlnp-uw-zABD-HJLNP-UW-Z]{2}))");

        public static LegalEntity MapFromAccountApiLegalEntity(LegalEntityViewModel data)
        {
            return new LegalEntity
            {
                LegalEntityId = data.LegalEntityId,
                Name = data.Name,
                Code = data.Code,
                Status = data.Status,
                Address = MapFromAddressLine(data.Address)
            };
        }

        internal static Address MapFromAddressLine(string address)
        {
            const string splitChar = ",";

            if (!string.IsNullOrWhiteSpace(address) && address.Contains(splitChar) && PostcodeRegex.IsMatch(address))
            {
                var addressParts = address.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim());

                var postcode = addressParts.FirstOrDefault(x => PostcodeRegex.IsMatch(x));
                var nonPostcodeAddressParts = addressParts.Where(x => !PostcodeRegex.IsMatch(x));

                var legalEntityAddress = new Address
                {
                    AddressLine1 = nonPostcodeAddressParts.First(),
                    AddressLine2 = nonPostcodeAddressParts.ElementAtOrDefault(1),
                    AddressLine3 = nonPostcodeAddressParts.ElementAtOrDefault(2),
                    AddressLine4 = nonPostcodeAddressParts.ElementAtOrDefault(3),
                    Postcode = postcode
                };

                return legalEntityAddress;
            }

            if (PostcodeRegex.IsMatch(address))
            {
                return new Address { Postcode = address };
            }

            return new Address { AddressLine1 = address };
        }
    }
}
