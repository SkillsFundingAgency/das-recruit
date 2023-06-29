using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.VacancySummariesProvider
{
    internal class VacancyApplicationsDashboardResponseDto
    {
        public VacancyApplicationsDashboardDto Id { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public int StatusCount { get; set; }
    }

    internal class VacancySharedApplicationsDashboardResponseDto
    {
        public VacancyApplicationsDashboardDto Id { get; set; }
        public int NoOfSharedApplications { get; set; }
    }


    internal class VacancyApplicationsDashboardDto
    {
        public VacancyStatus Status { get; set; }
        public bool ClosingSoon { get; set; }
    }
}