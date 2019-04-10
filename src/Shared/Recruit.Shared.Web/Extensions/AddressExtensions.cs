using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Recruit.Shared.Web.Extensions
{
    public static class AddressExtensions
    {
        public static string ToAddressString(this IAddress address)
        {
            return string
                .Join(", ", new[] {address.AddressLine1, address.AddressLine2, address.AddressLine3, address.AddressLine4, address.Postcode })
                .Replace(" ,", string.Empty);
        }
    }
}