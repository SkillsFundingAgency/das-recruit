using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyService
    {
        Task CloseVacancy(Guid vacancyId);

        Task PerformRulesCheckAsync(VacancyReview vacancy);
    }
}