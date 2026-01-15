using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyReviewRepository
    {
        Task CreateAsync(VacancyReview vacancyReview);
        Task UpdateAsync(VacancyReview review);
    }
}