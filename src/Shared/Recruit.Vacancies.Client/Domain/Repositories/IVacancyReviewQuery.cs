using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyReviewQuery
    {
        Task<List<VacancyReview>> GetForVacancyAsync(long vacancyReference);
        Task<VacancyReview> GetLatestReviewByReferenceAsync(long vacancyReference);
        Task<List<VacancyReview>> GetByStatusAsync(ReviewStatus status);
        Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);
        Task<VacancyReview> GetAsync(Guid reviewId);
    }
}