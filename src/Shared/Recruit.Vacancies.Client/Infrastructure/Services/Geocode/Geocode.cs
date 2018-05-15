namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public class Geocode
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public override string ToString()
        {
            return $"{{\"Latitude\":{Latitude}, \"Longitude\":{Longitude}}}";
        }
    }
}
