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
    }
}
