using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;
using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities
{
    public class Vacancy : 
        IRoleDescriptionPatch, 
        ICreateVacancyPatch
    {
        public Guid Id { get; set; }
        
        public string Title { get; set; }
        
        public DateTime? AuditVacancyCreated { get; set; }
    }
}
