namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetProfanitiesRequest: IGetApiRequest
{
    public string GetUrl => "prohibitedContent/profanities";
}