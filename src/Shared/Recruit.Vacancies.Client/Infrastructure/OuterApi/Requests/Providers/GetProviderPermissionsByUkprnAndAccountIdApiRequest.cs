using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Providers;

public record GetProviderPermissionsByUkprnAndAccountIdApiRequest(long Ukprn, long AccountId, List<OperationType> Operations) : IGetApiRequest
{
    public string GetUrl => $"accountlegalentities/provider/{Ukprn}/employerAccount/{AccountId}?operations={string.Join("&operations=", Operations)}";
}