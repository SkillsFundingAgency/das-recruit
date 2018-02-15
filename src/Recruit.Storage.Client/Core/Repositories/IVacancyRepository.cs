using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public interface IVacancyRepository
    {
        Task CreateVacancyAsync(Vacancy vacancy);
        Task<Vacancy> GetVacancyAsync(Guid id);
        Task UpdateVacancyAsync(Vacancy vacancy);
    }
}