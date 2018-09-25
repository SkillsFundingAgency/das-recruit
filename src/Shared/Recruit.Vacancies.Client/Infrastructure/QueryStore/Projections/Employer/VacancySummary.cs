using System; 
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.Employer
{
    public class VacancySummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public long? VacancyReference { get; internal set; }
        public DateTime? CreatedDate { get; set; }
        public VacancyStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public int AllApplicationsCount { get; set; }
        public int NewApplicationsCount { get; set; }
        public DateTime? ClosingDate { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }

        public bool HasVacancyReference => VacancyReference.HasValue;
        public bool HasNoVacancyReference => !HasVacancyReference;
        public bool IsApplicationsVacancy => (Status== VacancyStatus.Live || Status ==VacancyStatus.Closed) 
                                             && ApplicationMethod == Domain.Entities.ApplicationMethod.ThroughFindAnApprenticeship;

        public bool HasApplications => AllApplicationsCount > 0;
        public bool HasNoApplications => !HasApplications;
    }
}