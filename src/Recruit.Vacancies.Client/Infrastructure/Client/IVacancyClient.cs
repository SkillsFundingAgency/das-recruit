using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Client
{
    public interface IVacancyClient
    {
        Task<Vacancy> GetVacancyForEditAsync(Guid id);
        Task<Guid> CreateVacancyAsync(string title, string employerAccountId);
        Task UpdateVacancyAsync(Vacancy vacancy);
        Task<bool> SubmitVacancyAsync(Guid id);
    }
}