using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    internal static class VacancySummaryMapper
    {
        internal static VacancySummary MapFromVacancySummaryAggQueryResponseDto(VacancySummaryAggQueryResponseDto src)
        {
            var vacSummaryDetail = src.Id;

            return new VacancySummary
            {
                Id = vacSummaryDetail.VacancyGuid,
                Title = vacSummaryDetail.Title,
                VacancyReference = vacSummaryDetail.VacancyReference,
                LegalEntityId = vacSummaryDetail.LegalEntityId,
                LegalEntityName = vacSummaryDetail.LegalEntityName,
                EmployerAccountId = vacSummaryDetail.EmployerAccountId,
                EmployerName = vacSummaryDetail.EmployerName,
                Ukprn = vacSummaryDetail.Ukprn,
                CreatedDate = vacSummaryDetail.CreatedDate,
                Duration = vacSummaryDetail.Duration,
                DurationUnit = vacSummaryDetail.DurationUnit,
                Status = vacSummaryDetail.Status,
                ClosingDate = vacSummaryDetail.ClosingDate,
                ApplicationMethod = vacSummaryDetail.ApplicationMethod,
                ProgrammeId = vacSummaryDetail.ProgrammeId,
                StartDate = vacSummaryDetail.StartDate,
                TrainingTitle = vacSummaryDetail.TrainingTitle,
                TrainingType = vacSummaryDetail.TrainingType,
                TrainingLevel = vacSummaryDetail.TrainingLevel,

                NoOfNewApplications = src.NoOfNewApplications,
                NoOfSuccessfulApplications = src.NoOfSuccessfulApplications,
                NoOfUnsuccessfulApplications = src.NoOfUnsuccessfulApplications
            };
        }
    }
}