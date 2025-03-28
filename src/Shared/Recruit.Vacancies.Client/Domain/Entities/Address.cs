using System;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Address : IAddress, IEquatable<Address>
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
        public string Country { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool HasGeocode => Latitude.HasValue && Longitude.HasValue;

        public bool Equals(Address other)
        {
            if (other is null)
            {
                return false;
            }
            
            return ReferenceEquals(this, other) || string.Equals(this.Flatten(), other.Flatten(), StringComparison.OrdinalIgnoreCase);
        }
    }
}
