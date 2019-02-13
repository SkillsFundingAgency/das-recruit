using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections
{
    public class VacancySummary
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
        public string EmployerName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public VacancyStatus Status { get; set; }
        public bool IsDeleted { get; set; }
        public int AllApplicationsCount { get; set; }
        public int NewApplicationsCount { get; set; }
        public DateTime? ClosingDate { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ProgrammeId { get; set; }
        public string TrainingTitle { get; set; }
        public TrainingType TrainingType { get; set; }
        public ProgrammeLevel TrainingLevel { get; set; }
    }
}