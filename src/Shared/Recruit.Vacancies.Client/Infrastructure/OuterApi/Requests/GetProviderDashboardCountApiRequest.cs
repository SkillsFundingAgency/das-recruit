namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public record GetProviderDashboardCountApiRequest(long Ukprn, string UserId) : IGetApiRequest
    {
        public string GetUrl
        {
            get
            {
                return $"providers/{Ukprn}/dashboard?userId={UserId}";
            }
        }
    }
}