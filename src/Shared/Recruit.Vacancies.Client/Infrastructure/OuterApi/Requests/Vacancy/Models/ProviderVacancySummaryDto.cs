using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Models;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Requests.Vacancy.Models;

public class ProviderVacancySummaryDto
{
    public static ProviderVacancySummary ToDomain(ProviderVacancySummaryDto source)
    {
        return new ProviderVacancySummary()
        {
            EmployerAccountId = source.AccountId,
            Id = source.Id,
            Status = source.Status,
            VacancyOwner = source.VacancyOwner,
            VacancyReference = source.VacancyReference ?? 0
        };
    }

    public Guid Id { get; set; }
    public OwnerType? VacancyOwner { get; set; }
    public string AccountId { get; set; }
    public long? VacancyReference { get; set; }
    public VacancyStatus Status { get; set; }
}