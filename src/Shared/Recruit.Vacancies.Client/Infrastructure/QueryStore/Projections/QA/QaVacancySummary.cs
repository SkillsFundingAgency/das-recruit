using System;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.QueryStore.Projections.QA
{
    public class QaVacancySummary
    {
        public Guid Id { get; set; }

        public string EmployerName { get; set; }

        public string Title { get; set; }

        public long VacancyReference { get; set; }

        public DateTime SubmittedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public DateTime? ReviewStartedOn { get; set; }

        public string ReviewAssignedToUserName { get; set; }

        public string ReviewAssignedToUserId { get; set; }
    }
}
