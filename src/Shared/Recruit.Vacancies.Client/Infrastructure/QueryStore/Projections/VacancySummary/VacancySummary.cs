using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections
{
    public class VacancySummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
        public string LegalEntityName { get; set; }
        public string EmployerAccountId { get; set; }
        public string EmployerName { get; set; }
        public long? Ukprn { get; set; }
        public DateTime? CreatedDate { get; set; }
        public VacancyStatus Status { get; set; }
        public DateTime? ClosingDate { get; set; }
        public int? Duration { get; set; }
        public DurationUnit? DurationUnit { get; internal set; }
        public DateTime? ClosedDate { get; set; }
        public ClosureReason? ClosureReason { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ProgrammeId { get; set; }
        public DateTime? StartDate { get; set; }
        public string TrainingTitle { get; set; }
        public TrainingType TrainingType { get; set; }
        public ApprenticeshipLevel TrainingLevel { get; set; }
        public long? TransferInfoUkprn { get; set; }
        public string TransferInfoProviderName { get; set; }
        public TransferReason? TransferInfoReason { get; set; }
        public DateTime? TransferInfoTransferredDate { get; set; }
        public string TrainingProviderName { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public int NoOfApplications => NoOfNewApplications + NoOfSuccessfulApplications + NoOfUnsuccessfulApplications;
    }
}