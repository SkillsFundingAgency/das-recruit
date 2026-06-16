using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Providers;

public record GetProviderPermissionsByUkprnApiRequest(long Ukprn, List<OperationType> Operations) : IGetApiRequest
{
    public string GetUrl => $"accountLegalEntities/provider/{Ukprn}?operations={string.Join("&operations=", Operations)}";
}