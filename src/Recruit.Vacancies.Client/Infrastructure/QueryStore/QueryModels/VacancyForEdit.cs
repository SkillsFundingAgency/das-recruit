using System;

namespace Recruit.Vacancies.Client.Infrastructure.QueryStore.QueryModels
{
    public class VacancyForEdit
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public DateTime? AuditVacancyCreated { get; set; }
    }
}