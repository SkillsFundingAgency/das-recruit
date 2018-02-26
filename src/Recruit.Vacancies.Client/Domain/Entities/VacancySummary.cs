using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class VacancySummary
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public DateTime? CreatedDate { get; set; }

        public VacancyStatus Status { get; set; }

        public DateTime? SubmittedDate { get; set; }
    }
}