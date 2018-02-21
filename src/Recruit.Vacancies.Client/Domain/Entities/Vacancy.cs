using System;

namespace Esfa.Recruit.Storage.Client.Domain.Entities
{
    public class Vacancy
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public DateTime? AuditVacancyCreated { get; set; }
        public global::System.String EmployerAccountId { get; internal set; }
    }
}
