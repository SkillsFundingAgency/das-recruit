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
    }
}
