using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public static class LegalEntityMapper
    {
        public static LegalEntity MapFromAccountApiLegalEntity(AccountLegalEntity data)
        {
            return new LegalEntity
            {
                AccountLegalEntityPublicHashedId = data.AccountLegalEntityPublicHashedId,
                Name = data.Name,
                Address = MapFromAddressLine(data.Address),
                HasLegalEntityAgreement = data.HasLegalAgreement
            };
        }

        internal static Address MapFromAddressLine(string address)
        {
            const string splitChar = ",";

            if (string.IsNullOrWhiteSpace(address))
            {
                return new Address();
            }
            
            if (!string.IsNullOrWhiteSpace(address) && address.Contains(splitChar) && ValidationConstants.PostcodeRegex.IsMatch(address))
            {
                var addressParts = address.Split(new[] { splitChar }, StringSplitOptions.RemoveEmptyEntries)
                                            .Select(s => s.Trim());

                var postcode = addressParts.FirstOrDefault(x => ValidationConstants.PostcodeRegex.IsMatch(x));
                var nonPostcodeAddressParts = addressParts.Where(x => !ValidationConstants.PostcodeRegex.IsMatch(x));

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

            return ValidationConstants.PostcodeRegex.IsMatch(address) ? new Address { Postcode = address } : new Address { AddressLine1 = address };
        }
    }
}
