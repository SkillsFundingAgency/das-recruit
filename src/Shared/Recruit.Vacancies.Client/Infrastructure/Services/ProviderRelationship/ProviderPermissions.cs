using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.ProviderRelationship;

public class ProviderPermissions
{
    public IEnumerable<LegalEntityDto> AccountProviderLegalEntities { get; set; }
}