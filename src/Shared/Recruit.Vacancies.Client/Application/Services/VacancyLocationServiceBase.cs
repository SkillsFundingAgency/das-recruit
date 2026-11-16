using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Locations;

namespace Esfa.Recruit.Vacancies.Client.Application.Services;

public abstract class VacancyLocationServiceBase(ILocationsService locationsService)
{
    public async Task<Address> EnrichAddressWithGeoData(Address address)
    {
        if (address is null || string.IsNullOrEmpty(address.Postcode) || address.HasGeocode)
            return address;

        var postcodeData = await locationsService.GetPostcodeDataAsync(address.Postcode);

        if (postcodeData is null)
            return address;

        address.Latitude = postcodeData.Latitude;
        address.Longitude = postcodeData.Longitude;
        address.Country = postcodeData.Country;

        return address;
    }
}