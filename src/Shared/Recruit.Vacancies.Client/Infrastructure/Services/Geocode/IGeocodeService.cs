using System;
using System.Collections.Generic;
using System.Text;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode
{
    public interface IGeocodeService
    {
        Geocode Geocode(string postcode);
    }
}
