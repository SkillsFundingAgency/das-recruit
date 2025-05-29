using System;
using Newtonsoft.Json;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses;

public class GetCandidateDetailApiResponse
{
    public class GetCandidateApplicationApiResponse
    {
        
        [JsonProperty("dateOfBirth")]
        public DateTime DateOfBirth { get; set; }

        [JsonProperty("id")]
        public Guid Id { get; set; }
    }
}