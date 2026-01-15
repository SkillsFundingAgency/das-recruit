namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Reports;
public record GetProviderReportsApiRequest(long Ukprn) : IGetApiRequest
{
    public string GetUrl => $"reports/{Ukprn}/provider";
}