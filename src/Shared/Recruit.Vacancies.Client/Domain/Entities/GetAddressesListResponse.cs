using System.Collections.Generic;
using System.Linq;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
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

        public List<string> AddressLines =>
                new string[]
                {
                    string.Join(" ", new string[] { House, Street }.Where(p => !string.IsNullOrEmpty(p))),
                    Locality
                }.Where(p => !string.IsNullOrEmpty(p)).ToList();
            
       public string AddressLine1 => AddressLines.Count() >= 1 ? AddressLines.ElementAt(0) : string.Empty;

        public string AddressLine2 => AddressLines.Count() >= 2 ? AddressLines.ElementAt(1) : string.Empty;
    }
}
