using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public class GetUserAccountsResponse
    {
        public List<string> HashedAccountIds { get; set; }
    }
}