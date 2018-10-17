using System;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyService
    {
        Task CloseExpiredVacancy(Guid vacancyId);
        Task CloseVacancyImmediately(Guid vacancyId, VacancyUser user);
        Task PerformRulesCheckAsync(Guid reviewId);
    }
}