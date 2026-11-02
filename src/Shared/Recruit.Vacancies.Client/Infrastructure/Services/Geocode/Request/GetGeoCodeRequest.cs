using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.Geocode.Request;

public class GetGeoCodeRequest : IGetApiRequest
{
    private readonly string _postCode;

    public GetGeoCodeRequest(string postCode)
    {
        _postCode = postCode;
    }

    string IGetApiRequest.GetUrl => $"locations/geopoint?postcode={_postCode}";
}