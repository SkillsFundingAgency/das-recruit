using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections
{
    public class VacancyDashboard
    {
        public List<VacancyStatusDashboard> VacancyStatusDashboard { get; set; }
        public List<VacancyApplicationsDashboard> VacancyApplicationsDashboard { get; set; }
        public int VacanciesClosingSoonWithNoApplications { get; set; }
    }

    public class VacancyStatusDashboard
    {
        public VacancyStatus Status { get; set; }
        public int StatusCount { get; set;  }
        public bool ClosingSoon { get; set; }
    }

    public class VacancyApplicationsDashboard
    {
        public VacancyStatus Status { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public bool ClosingSoon { get; set; }
        public int StatusCount { get; set; }
    }
}