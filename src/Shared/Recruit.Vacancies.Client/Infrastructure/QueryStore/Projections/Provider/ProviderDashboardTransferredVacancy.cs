using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Provider;

public class ProviderDashboardTransferredVacancy
{
    public string LegalEntityName { get; set; }
    public DateTime TransferredDate { get; set; }
    public TransferReason Reason { get; set; }
}