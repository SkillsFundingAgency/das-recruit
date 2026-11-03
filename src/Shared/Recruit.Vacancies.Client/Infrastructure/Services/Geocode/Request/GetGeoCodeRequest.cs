using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Request;

public class GetGeoCodeRequest(string postCode) : IGetApiRequest
{
    string IGetApiRequest.GetUrl => $"locations/geopoint?postcode={postCode}";
}