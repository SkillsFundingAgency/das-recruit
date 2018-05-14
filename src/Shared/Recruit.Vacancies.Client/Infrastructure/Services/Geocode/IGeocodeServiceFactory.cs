namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public interface IGeocodeServiceFactory
    {
        IGeocodeService GetGeocodeService();
    }
}