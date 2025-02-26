using System.Linq;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.Domain;

public static class GetAddressesListItemExtensions
{
    public static string ToShortAddress(this GetAddressesListItem item)
    {
        return string.Join(", ", new[]
        {
            item.AddressLine1,
            item.AddressLine2,
        }.Where(x => !string.IsNullOrEmpty(x)));
    }
}