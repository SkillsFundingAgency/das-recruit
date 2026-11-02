using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode;

public interface IOuterApiGeocodeService
{
    Task<Geocode> Geocode(string postcode);
}