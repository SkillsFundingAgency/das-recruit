namespace Esfa.Recruit.Employer.Web.Services
{
    public interface IGeocodeImageService
    {
        string GetMapImageUrl(string postcode);
        string GetMapImageUrl(string latitude, string longitude);
    }
}
