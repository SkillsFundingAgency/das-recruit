namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests;

public class GetProviderRequest : IGetApiRequest
{
    private readonly long _ukprn;

    public GetProviderRequest(long ukprn)
    {
        _ukprn = ukprn;
    }

    public string GetUrl => $"providers/{_ukprn}";
}