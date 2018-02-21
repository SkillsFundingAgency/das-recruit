using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches
{
    public interface ICreateVacancyPatch : IVacancyPatch
    {
        string Title { get; set; }
        string EmployerAccountId { get; set; }
        DateTime? AuditVacancyCreated { get; set; }
    }
}
