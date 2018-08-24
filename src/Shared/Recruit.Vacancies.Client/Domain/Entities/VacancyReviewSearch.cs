using System;
using System.Collections.Generic;
using System.Text;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class VacancyReviewSearch
    {
        public Guid Id { get; set; }

        public string EmployerName { get; set; }

        public string Title { get; set; }

        public long VacancyReference { get; set; }

        public DateTime SubmittedDate { get; set; }

        public DateTime ClosingDate { get; set; }

        public string ReviewAssignedTo { get; set; }

        public DateTime? ReviewStartedOn { get; set; }
    }
}
