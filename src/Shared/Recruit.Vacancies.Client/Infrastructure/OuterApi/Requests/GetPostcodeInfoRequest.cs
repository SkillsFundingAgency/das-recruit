namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetPostcodeInfoRequest(string Postcode): IGetApiRequest
{
    public string GetUrl => $"postcodes/{Postcode}";
}