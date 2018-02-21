using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Domain.Entities;

namespace Esfa.Recruit.Storage.Client.Domain.Repositories
{
    public interface IVacancyRepository
    {
        Task CreateAsync(Vacancy vacancy);
        Task UpdateAsync(Vacancy vacancy);
        Task<Vacancy> GetVacancyAsync(Guid id);
    }
}