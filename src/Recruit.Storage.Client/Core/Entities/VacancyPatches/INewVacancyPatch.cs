using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches
{
    public interface INewVacancyPatch : IVacancyPatch
    {
        string Title { get; set; }

        DateTime? AuditVacancyCreated { get; set; }
    }
}
