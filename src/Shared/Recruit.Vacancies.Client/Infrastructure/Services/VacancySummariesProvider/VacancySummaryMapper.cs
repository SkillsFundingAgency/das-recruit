using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;

internal static class VacancySummaryMapper
{
    public static VacancySummary MapFromVacancySummaryAggQueryResponseDto(VacancySummaryAggQueryResponseDto src)
    {
        var vacSummaryDetail = src.Id;

        var vacancySummary = new VacancySummary
        {
            Id = vacSummaryDetail.VacancyGuid,
            Title = vacSummaryDetail.Title,
            VacancyReference = vacSummaryDetail.VacancyReference,
            LegalEntityName = vacSummaryDetail.LegalEntityName,
            EmployerAccountId = vacSummaryDetail.EmployerAccountId,
            EmployerName = vacSummaryDetail.EmployerName,
            Ukprn = vacSummaryDetail.Ukprn,
            CreatedDate = vacSummaryDetail.CreatedDate,
            Duration = vacSummaryDetail.Duration,
            DurationUnit = vacSummaryDetail.DurationUnit,
            Status = vacSummaryDetail.Status,
            ClosingDate = vacSummaryDetail.ClosingDate,
            ClosedDate = vacSummaryDetail.ClosedDate,
            ClosureReason = vacSummaryDetail.ClosureReason,
            ApplicationMethod = vacSummaryDetail.ApplicationMethod,
            ProgrammeId = vacSummaryDetail.ProgrammeId,
            StartDate = vacSummaryDetail.StartDate,
            TrainingTitle = vacSummaryDetail.TrainingTitle,
            TrainingType = vacSummaryDetail.TrainingType,
            TrainingLevel = vacSummaryDetail.TrainingLevel,
            TransferInfoUkprn = vacSummaryDetail.TransferInfoUkprn,
            TransferInfoProviderName = vacSummaryDetail.TransferInfoProviderName,
            TransferInfoReason = vacSummaryDetail.TransferInfoReason,
            TransferInfoTransferredDate = vacSummaryDetail.TransferInfoTransferredDate,
            TrainingProviderName = vacSummaryDetail.TrainingProviderName,
            NoOfNewApplications = src.NoOfNewApplications,
            NoOfSuccessfulApplications = src.NoOfSuccessfulApplications,
            NoOfUnsuccessfulApplications = src.NoOfUnsuccessfulApplications,
            NoOfSharedApplications = src.NoOfSharedApplications,
            NoOfAllSharedApplications = src.NoOfAllSharedApplications,
            NoOfEmployerReviewedApplications = src.NoOfEmployerReviewedApplications,
            IsTraineeship = vacSummaryDetail.IsTraineeship,
            IsTaskListCompleted = vacSummaryDetail.OwnerType is OwnerType.Provider or OwnerType.Employer && vacSummaryDetail.HasSubmittedAdditionalQuestions,
            HasChosenProviderContactDetails = vacSummaryDetail.HasChosenProviderContactDetails,
            ApprenticeshipType = vacSummaryDetail.ApprenticeshipType,
        };

        return vacancySummary;
    }
}