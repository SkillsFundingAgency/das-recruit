using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public class GetUserAccountsResponse
    {
        public List<string> HashedAccountIds { get; set; }
    }
}