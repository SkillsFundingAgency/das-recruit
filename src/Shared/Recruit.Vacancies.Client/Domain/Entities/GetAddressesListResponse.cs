using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class GetAddressesListResponse
{
    public IEnumerable<GetAddressesListItem> Addresses { get; set; }
}

public class GetAddressesListItem
{
    public string Uprn { get; set; }
    public string House { get; set; }
    public string Street { get; set; }
    public string Locality { get; set; }
    public string PostTown { get; set; }
    public string County { get; set; }
    public string Postcode { get; set; }
    public string Country { get; set; }
    public double? Longitude { get; set; }
    public double? Latitude { get; set; }
    public double? Match { get; set; }

    public string Text
    {
        get
        {
            return string.Join(", ", (new string[] { AddressLine1, AddressLine2, PostTown, County, Postcode }.Where(p => !string.IsNullOrEmpty(p))));
        }
    }

    public string Thoroughfare { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2AndLine3 => string.Join(", ", new string[] { AddressLine2, AddressLine3 }.Where(p => !string.IsNullOrEmpty(p)));
    public string AddressLine2 { get; set; }
    public string AddressLine3 { get; set; }
}