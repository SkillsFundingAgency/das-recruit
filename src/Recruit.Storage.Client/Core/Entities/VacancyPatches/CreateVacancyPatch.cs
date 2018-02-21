using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches
{
    public class CreateVacancyPatch : ICreateVacancyPatch
    {
        public string Title { get; set; }
        public string EmployerAccountId { get; set; }
        public DateTime? AuditVacancyCreated { get; set; }
    }
}
