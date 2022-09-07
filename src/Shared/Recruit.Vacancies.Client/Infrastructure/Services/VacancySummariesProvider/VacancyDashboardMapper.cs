using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    internal static class VacancyDashboardMapper
    {
        internal static VacancyDashboard MapFromVacancyDashboardSummaryResponseDto(VacancyDashboardAggQueryResponseDto src)
        {
            return new VacancyDashboard
            {
                Status = src.Id.Status,
                StatusCount = src.StatusCount,
                ClosingSoon = src.Id.ClosingSoon,
                NoOfNewApplications = src.NoOfNewApplications,
                NoOfSuccessfulApplications = src.NoOfSuccessfulApplications,
                NoOfUnsuccessfulApplications = src.NoOfUnsuccessfulApplications
            };
        }
    }
}