using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Interfaces;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;

internal class VacancySummaryAggQueryResponseDto
{
    public VacancySummaryDetails Id { get; set; }
    public int NoOfNewApplications { get; set; }
    public int NoOfSuccessfulApplications { get; set; }
    public int NoOfUnsuccessfulApplications { get; set; }
    public int NoOfSharedApplications { get; set; }
    public int NoOfAllSharedApplications { get; set; }
    public int NoOfEmployerReviewedApplications { get; set; }
}

internal class VacancySummaryDetails :  ITaskListVacancy
{
    public Guid VacancyGuid { get; set; }
    public long? VacancyReference { get; set; }
    public string Title { get; set; }
    public string LegalEntityName { get; set; }
    public string EmployerAccountId { get; set; }
    public string EmployerName { get; set; }
        
    public string EmployerDescription { get; set; }
    public long? Ukprn { get; set; }
    public DateTime? CreatedDate { get; set; }
    public VacancyStatus Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? ClosingDate { get; set; }
    public DateTime? ClosedDate { get; set; }
    public ClosureReason? ClosureReason { get; set; }
    public ApplicationMethod? ApplicationMethod { get; set; }
    public string ProgrammeId { get; set; }
    public int? Duration { get; set; }
    public DurationUnit? DurationUnit { get; set; }
    public string TrainingTitle { get; set; }
    public TrainingType TrainingType { get; set; }
    public ApprenticeshipLevel TrainingLevel { get; set; }
    public long? TransferInfoUkprn { get; set; }
    public string TransferInfoProviderName { get; set; }
    public DateTime? TransferInfoTransferredDate { get; set; }
    public TransferReason? TransferInfoReason { get; set; }
    public string TrainingProviderName { get; set; }
    public bool IsTraineeship { get; set; }
    public bool? HasChosenProviderContactDetails { get; set; }
    public bool HasSubmittedAdditionalQuestions { get; set; }
    public OwnerType OwnerType { get; set; }
    public ApprenticeshipTypes? ApprenticeshipType { get; set; }
}