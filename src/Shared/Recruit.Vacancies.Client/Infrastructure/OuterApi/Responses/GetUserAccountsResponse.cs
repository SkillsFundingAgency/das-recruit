using System.Collections.Generic;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses
{
    public class GetUserAccountsResponse
    {
        [JsonProperty]
        public string EmployerUserId { get; set; }
        [JsonProperty]
        public string FirstName { get; set; }
        [JsonProperty]
        public string LastName { get; set; }
        [JsonProperty("UserAccounts")]
        public List<EmployerIdentifier> UserAccounts { get; set; }
    }
    
    public class EmployerIdentifier
    {
        [JsonProperty("EncodedAccountId")]
        public string AccountId { get; set; }
        [JsonProperty("DasAccountName")]
        public string EmployerName { get; set; }
        [JsonProperty("Role")]
        public string Role { get; set; }
    }
}