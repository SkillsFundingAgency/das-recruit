using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests
{
    public record GetEmployerApplicationReviewsCountApiRequest(long AccountId, List<long> VacancyReferences) : IPostApiRequest
    {
        public string PostUrl
        {
            get
            {
                return $"employerAccounts/{AccountId}/count";
            }
        }

        public object Data { get; set; } = VacancyReferences;
    }
}