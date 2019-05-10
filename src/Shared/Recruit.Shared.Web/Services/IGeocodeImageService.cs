namespace Esfa.Recruit.Shared.Web.Services
{
    public interface IGeocodeImageService
    {
        string GetMapImageUrl(string postcode, int imageWidth, int imageHeight, bool showMarker);
        string GetMapImageUrl(string latitude, string longitude, int imageWidth, int imageHeight, bool showMarker);
    }
}
