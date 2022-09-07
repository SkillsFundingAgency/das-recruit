using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections
{
    public class VacancyDashboard
    {
        public VacancyStatus Status { get; set; }
        public int StatusCount { get; set;  }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public bool ClosingSoon { get; set; }
    }
}