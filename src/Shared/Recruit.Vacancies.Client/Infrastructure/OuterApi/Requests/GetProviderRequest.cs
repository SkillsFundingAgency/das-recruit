namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetProviderRequest(long ukprn) : IGetApiRequest
{
    public string GetUrl => $"providers/{ukprn}";
}