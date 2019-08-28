using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Application.Services
{
    public interface IVacancyService
    {
        Task CloseExpiredVacancy(Guid vacancyId);
        Task PerformRulesCheckAsync(Guid reviewId);
    }
}