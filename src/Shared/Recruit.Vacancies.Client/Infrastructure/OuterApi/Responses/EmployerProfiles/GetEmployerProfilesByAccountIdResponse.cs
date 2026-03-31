using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.EmployerProfiles;

public record GetEmployerProfilesByAccountIdResponse
{
    public List<EmployerProfile> EmployerProfiles { get; set; } = [];
}