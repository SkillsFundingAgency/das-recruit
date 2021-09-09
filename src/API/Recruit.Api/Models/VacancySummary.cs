using System;

namespace SFA.DAS.Recruit.Api.Models
{
    public class VacancySummary
    {
        public string Title { get; set; }
        public long? VacancyReference { get; set; }
        public long? LegalEntityId { get; set; }
        public string LegalEntityName { get; set; }
        public string EmployerAccountId { get; set; }
        public string EmployerName { get; set; }
        public long? Ukprn { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string Status { get; set; }
        public DateTime? ClosingDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public int? Duration { get; set; }
        public string DurationUnit { get; set; }
        public string ApplicationMethod { get; set; }
        public string ProgrammeId { get; set; }
        public DateTime? StartDate { get; set; }
        public string TrainingTitle { get; set; }
        public string TrainingType { get; set; }
        public string TrainingLevel { get; set; }
        public int? NoOfNewApplications { get; set; }
        public int? NoOfSuccessfulApplications { get; set; }
        public int? NoOfUnsuccessfulApplications { get; set; }

        public string FaaVacancyDetailUrl { get; set; }
        public string RaaManageVacancyUrl  { get; set; }
    }
}