using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi.Responses.Vacancies;

public record VacancyListItem(
    Guid Id,
    long? VacancyReference,
    string Title,
    string LegalEntityName,
    DateTime? ClosingDate,
    VacancyStatus Status,
    SourceOrigin? SourceOrigin,
    ApprenticeshipTypes? ApprenticeshipType,
    OwnerType? OwnerType,
    ApplicationMethod? ApplicationMethod,
    bool? HasSubmittedAdditionalQuestions,
    string? TransferInfo,
    VacancyStatsItem? Stats)
{
    public static implicit operator VacancyListItem(VacancySummary summary) =>
        new(summary.Id,
            summary.VacancyReference,
            summary.Title,
            summary.LegalEntityName,
            summary.ClosingDate,
            summary.Status,
            summary.SourceOrigin,
            summary.ApprenticeshipType,
            summary.OwnerType,
            summary.ApplicationMethod,
            summary.HasSubmittedAdditionalQuestions,
            summary.TransferInfo,
            new VacancyStatsItem(summary.NoOfApplications,
                summary.NoOfNewApplications,
                summary.NoOfSharedApplications,
                summary.NoOfAllSharedApplications,
                summary.NoOfSuccessfulApplications,
                summary.NoOfUnsuccessfulApplications,
                summary.NoOfEmployerReviewedApplications));
}