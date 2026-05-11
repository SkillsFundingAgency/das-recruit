using static Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles.PostEmployerProfileRequest;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles;

public record PostEmployerProfileRequest(long AccountLegalEntityId, PostEmployerProfileRequestData Payload) : IPostApiRequest
{
    public string PostUrl => $"employer/profiles/{AccountLegalEntityId}";
    public object Data { get; set; } = Payload;

    public sealed record PostEmployerProfileRequestData
    {
        public required long AccountId { get; init; }
        public string? AboutOrganisation { get; init; }
        public string? TradingName { get; init; }        
    }
}