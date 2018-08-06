using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyReviewRepository
    {
        Task CreateAsync(VacancyReview vacancy);
        Task<List<VacancyReview>> GetActiveAsync();
        Task<VacancyReview> GetAsync(Guid reviewId);
        Task UpdateAsync(VacancyReview review);
        Task<List<VacancyReview>> GetForVacancyAsync(long vacancyReference);
    }
}