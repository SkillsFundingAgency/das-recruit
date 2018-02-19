using System;

namespace Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches
{
    public class NewVacancyPatch : INewVacancyPatch
    {
        public string Title { get; set; }

        public DateTime? AuditVacancyCreated { get; set; }
    }
}
