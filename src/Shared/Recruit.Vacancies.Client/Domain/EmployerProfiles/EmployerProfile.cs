using System.Collections.Generic;

namespace Esfa.Recruit.Vacancies.Client.Domain.EmployerProfiles;

public record EmployerProfile(long AccountLegalEntityId,
    long AccountId,
    string? AboutOrganisation,
    string? TradingName,
    List<EmployerProfileAddress> Addresses);
