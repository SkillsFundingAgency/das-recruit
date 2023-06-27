using Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    internal static class VacancyDashboardApplicationsMapper
    {
        internal static VacancyApplicationsDashboard MapFromVacancyApplicationsDashboardResponseDto(VacancyApplicationsDashboardResponseDto src)
        {
            return new VacancyApplicationsDashboard
            {
                Status = src.Id.Status,
                NoOfNewApplications = src.NoOfNewApplications,
                NoOfSuccessfulApplications = src.NoOfSuccessfulApplications,
                NumberOfReviewedApplications = src.NoOfReviewedApplications,
                NoOfUnsuccessfulApplications = src.NoOfUnsuccessfulApplications,
                ClosingSoon = src.Id.ClosingSoon,
                StatusCount = src.StatusCount
            };
        }
    
    }
}