using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Providers;

public sealed record GetProviderPermissionsByUkprnAndAccountIdApiResponse
{
    public List<AccountLegalEntityItem> AccountProviderLegalEntities { get; set; } = [];
}