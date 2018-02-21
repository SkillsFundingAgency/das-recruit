using System;
using System.Threading.Tasks;
using Esfa.Recruit.Storage.Client.Domain.Entities;

namespace Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IVacancyClient
    {
        Task<Vacancy> GetVacancyForEditAsync(Guid id);
        Task<Guid> CreateVacancyAsync(string title, string employerAccountId);
        Task UpdateVacancyAsync(Vacancy vacancy);
    }
}