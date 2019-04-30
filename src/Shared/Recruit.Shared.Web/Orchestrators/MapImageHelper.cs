using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;

namespace Esfa.Recruit.Shared.Web.Orchestrators
{
    public static class MapImageHelper
    {
        public static string GetEmployerLocationMapUrl(Vacancy vacancy, IGeocodeImageService mapService, int width, int height)
        {
            if (vacancy.EmployerLocation.HasGeocode)
            {
                return mapService.GetMapImageUrl(
                    vacancy.EmployerLocation.Latitude.ToString(),
                    vacancy.EmployerLocation.Longitude.ToString(),
                    width, 
                    height,
                    vacancy.GeocodeUsingOutcode == false);
            }
            
            return vacancy.GeocodeUsingOutcode
                ? mapService.GetMapImageUrl(vacancy.EmployerLocation.PostcodeAsOutcode(), width, height, false)
                : mapService.GetMapImageUrl(vacancy.EmployerLocation.Postcode, width, height, true);
        }
    }
}
