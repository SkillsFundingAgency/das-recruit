using System.Text.Encodings.Web;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetPostcodeDataRequest(string Postcode): IGetApiRequest
{
    public string GetUrl => $"postcodes/{HtmlEncoder.Default.Encode(Postcode)}";
}