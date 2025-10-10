namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetBannedPhrasesRequest: IGetApiRequest
{
    public string GetUrl => "prohibitedContent/bannedPhrases";
}