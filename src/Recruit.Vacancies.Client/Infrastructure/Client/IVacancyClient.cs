using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Domain.Entities;

namespace Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IVacancyClient
    {
        Task<Vacancy> GetVacancyForEditAsync(Guid id);
        Task CreateVacancyAsync(Vacancy vacancy);
        Task UpdateVacancyAsync(Vacancy vacancy);
    }
}