using Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.EmployerProfiles;

public record GetEmployerProfilesByLegalEntityIdResponse
{
    public EmployerProfile? EmployerProfile { get; set; } = null;
}