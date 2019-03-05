using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Reports
{
    public class ReportDashboardOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ReportDashboardOrchestrator(IRecruitVacancyClient vacancyClient)
        {
            _vacancyClient = vacancyClient;
        }
    }
}
