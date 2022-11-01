using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Services.FAA
{
    public class FaaApplicationStatusSummary
    {
        public FaaApplicationStatus ApplicationStatus { get; set; }
        public string UnsuccessfulReason { get; set; }
        public bool IsRecruitVacancy { get; set; }
        public int? VacancyReference { get; set; }
        public Guid? CandidateId { get; set; }
    }
}
