using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities.VacancyPatches;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public interface ICommandVacancyRepository
    {
        Task UpsertVacancyAsync(Guid vacancyId, IVacancyPatch patch);
    }
}