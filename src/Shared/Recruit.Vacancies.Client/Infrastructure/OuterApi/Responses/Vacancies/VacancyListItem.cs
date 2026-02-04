using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

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
    VacancyStatsItem? Stats);