namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.EmployerProfiles;

public record GetEmployerProfileByLegalEntityIdRequest(long AccountLegalEntityId) : IGetApiRequest
{
    public string GetUrl => $"employer//profiles/{AccountLegalEntityId}";
}