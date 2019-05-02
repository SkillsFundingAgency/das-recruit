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
        public DateTime? ClosingDate { get; set; }
        public ApplicationMethod? ApplicationMethod { get; set; }
        public string ProgrammeId { get; set; }
        public string TrainingTitle { get; set; }
        public TrainingType TrainingType { get; set; }
        public ProgrammeLevel TrainingLevel { get; set; }
        public int NoOfNewApplications { get; set; }
        public int NoOfSuccessfulApplications { get; set; }
        public int NoOfUnsuccessfulApplications { get; set; }
        public int NoOfApplications => NoOfNewApplications + NoOfSuccessfulApplications + NoOfUnsuccessfulApplications;
    }
}