using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public interface IGeocodeService
    {
        Task<Geocode> Geocode(string postcode);
    }
}
