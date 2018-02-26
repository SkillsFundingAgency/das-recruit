using Esfa.Recruit.Vacancies.Client.Domain.Enums;
using System;

namespace Esfa.Recruit.Vacancies.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public string EmployerAccountId { get; internal set; }

        public DateTime? CreatedDate { get; set; }

        public VacancyStatus Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedDate { get; set; }

        /// <summary>
        /// We can only submit draft vacancies that have not been deleted
        /// </summary>
        public bool CanSubmit => Status == VacancyStatus.Draft && IsDeleted == false;

        /// <summary>
        /// We can only delete draft vacancies that have not been deleted
        /// </summary>
        public bool CanDelete => Status == VacancyStatus.Draft && IsDeleted == false;
    }
}
