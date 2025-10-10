namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public record GetAlertsByUkprnApiRequest(int Ukprn, string UserId) : IGetApiRequest
{
    public string GetUrl => $"providers/{Ukprn}/vacancies?userId={UserId}";
}