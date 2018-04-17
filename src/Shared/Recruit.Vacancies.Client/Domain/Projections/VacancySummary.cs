using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Domain.Projections
{
    public class VacancySummary
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public DateTime? CreatedDate { get; set; }

        public VacancyStatus Status { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? SubmittedDate { get; set; }
    }
}