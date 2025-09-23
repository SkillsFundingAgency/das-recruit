namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public record GetEmployerDashboardCountApiRequest(long AccountId, string UserId) : IGetApiRequest
    {
        public string GetUrl
        {
            get
            {
                return $"employerAccounts/{AccountId}/dashboard?userId={UserId}";
            }
        }
    }
}