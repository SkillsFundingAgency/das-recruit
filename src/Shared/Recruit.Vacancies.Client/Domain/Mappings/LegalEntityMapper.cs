using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using SFA.DAS.EAS.Account.Api.Types;
using System;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Domain.Mappings
{
    public static class LegalEntityMapper
    {
        public static LegalEntity MapFromAccountApiLegalEntity(LegalEntityViewModel data)
        {
            return new LegalEntity
            {
                LegalEntityId = data.LegalEntityId,
                Name = data.Name,
                Code = data.Code,
                Address = MapFromAddressLine(data.Address)
            };
        }

        internal static Address MapFromAddressLine(string address)
        {
            const string splitChar = ",";
            var hasPostcode = ValidationRegexes.PostcodeRegex.IsMatch(address);

            if (!string.IsNullOrWhiteSpace(address) && address.Contains(splitChar) && hasPostcode)
            {
                var addressParts = address.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim());

                var postcode = addressParts.FirstOrDefault(x => ValidationRegexes.PostcodeRegex.IsMatch(x));
                var nonPostcodeAddressParts = addressParts.Where(x => !ValidationRegexes.PostcodeRegex.IsMatch(x));

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

            if (hasPostcode)
            {
                return new Address { Postcode = address };
            }

            return new Address { AddressLine1 = address };
        }
    }
}
