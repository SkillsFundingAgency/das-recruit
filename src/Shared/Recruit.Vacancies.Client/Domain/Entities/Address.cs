namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string Postcode { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public bool HasGeocode => Latitude.HasValue && Longitude.HasValue;
        
        public override string ToString()
        {
            return string
                .Join(", ", new[] {AddressLine1, AddressLine2, AddressLine3, AddressLine4, Postcode })
                .Replace(" ,", string.Empty);
        }
    }
}
