using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Core.Entities;

namespace Esfa.Recruit.Storage.Client.Core.Repositories
{
    public interface IVacancyRepository
    {
        Task CreateVacancyAsync(MongoVacancy vacancy);
        Task<MongoVacancy> GetVacancyAsync(Guid id);
        Task<MongoVacancy> GetVacancyAsync(int vrn);
        Task UpdateVacancyAsync(MongoVacancy vacancy);
    }
}