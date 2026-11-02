namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Responses;

public class GetGeoPointResponse
{
    public GeoPoint GeoPoint { get; set; }
}

public class GeoPoint
{
    public string Postcode { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}