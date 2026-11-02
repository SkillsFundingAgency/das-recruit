using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider;

internal static class VacancyDashboardApplicationsMapper
{
    internal static VacancyApplicationsDashboard MapFromVacancyApplicationsDashboardResponseDto(VacancyApplicationsDashboardResponseDto src)
    {
        return new VacancyApplicationsDashboard
        {
            Status = src.Id.Status,
            NoOfNewApplications = src.NoOfNewApplications,
            NoOfSuccessfulApplications = src.NoOfSuccessfulApplications,
            NumberOfEmployerReviewedApplications = src.NoOfEmployerReviewedApplications,
            NoOfUnsuccessfulApplications = src.NoOfUnsuccessfulApplications,
            ClosingSoon = src.Id.ClosingSoon,
            StatusCount = src.StatusCount
        };
    }
    
}

internal static class VacancyDashboardSharedApplicationsMapper
{
    internal static VacancySharedApplicationsDashboard MapFromVacancySharedApplicationsDashboardResponseDto(VacancySharedApplicationsDashboardResponseDto src)
    {
        return new VacancySharedApplicationsDashboard
        {
            Status = src.Id.Status,
            NoOfSharedApplications = src.NoOfSharedApplications,
            NoOfAllSharedApplications = src.NoOfAllSharedApplications
        };
    }

}