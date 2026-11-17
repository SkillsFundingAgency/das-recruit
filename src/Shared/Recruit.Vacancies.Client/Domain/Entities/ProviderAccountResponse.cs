using System.Text.Json.Serialization;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities;

public class ProviderAccountResponse
{
    [JsonPropertyName("canAccessService")]
    public bool CanAccessService { get; set; }
}