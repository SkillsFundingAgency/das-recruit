using System;
using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.EditVacancyInfo;
using SFA.DAS.EAS.Account.Api.Types;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.EmployerAccount
{
    public static class LegalEntityMapper
    {
        public static LegalEntity MapFromAccountApiLegalEntity(LegalEntityViewModel data)
        {
            return new LegalEntity
            {
                LegalEntityId = data.LegalEntityId,
                AccountLegalEntityPublicHashedId = data.AccountLegalEntityPublicHashedId,
                Name = data.Name,
                Address = MapFromAddressLine(data.Address),
                HasLegalEntityAgreement = data.Agreements.Any(a =>
                    a.Status == EmployerAgreementStatus.Signed)
            };
        }

        internal static Address MapFromAddressLine(string address)
        {
            const string splitChar = ",";
            var hasPostcode = ValidationConstants.PostcodeRegex.IsMatch(address);

            if (!string.IsNullOrWhiteSpace(address) && address.Contains(splitChar) && hasPostcode)
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

            if (hasPostcode)
            {
                return new Address { Postcode = address };
            }

            return new Address { AddressLine1 = address };
        }
    }
}
