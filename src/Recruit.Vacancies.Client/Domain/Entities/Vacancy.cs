using System;

namespace Esfa.Recruit.Storage.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public string EmployerAccountId { get; internal set; }

        public DateTime? CreatedDate { get; set; }

        public VacancyStatus Status { get; set; }

        public DateTime? SubmittedDate { get; set; }

        public bool CanSubmit => Status == VacancyStatus.Draft;
    }
}
