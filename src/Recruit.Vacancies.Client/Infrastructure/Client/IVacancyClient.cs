using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Threading.Tasks;

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