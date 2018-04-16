namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IGeocodeImageService
    {
        string GetMapImageUrl(string postcode, int imageWidth, int imageHeight);
        string GetMapImageUrl(string latitude, string longitude, int imageWidth, int imageHeight);
    }
}
