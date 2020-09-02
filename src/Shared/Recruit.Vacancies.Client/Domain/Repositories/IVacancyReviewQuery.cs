using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IVacancyReviewQuery
    {
        Task<List<T>> GetActiveAsync<T>();
        Task<List<VacancyReview>> GetForVacancyAsync(long vacancyReference);
        Task<VacancyReview> GetLatestReviewByReferenceAsync(long vacancyReference);
        Task<List<VacancyReview>> GetByStatusAsync(ReviewStatus status);
        Task<List<VacancyReview>> GetVacancyReviewsInProgressAsync(DateTime getExpiredAssignationDateTime);
        Task<int> GetApprovedCountAsync(string submittedByUserId);
        Task<int> GetApprovedFirstTimeCountAsync(string submittedByUserId);
        Task<List<VacancyReview>> GetAssignedForUserAsync(string userId, DateTime assignationExpiryDateTime);
        Task<VacancyReview> GetCurrentReferredVacancyReviewAsync(long vacancyReference);
        Task<int> GetAnonymousApprovedCountAsync(string accountLegalEntityPublicHashedId);
    }
}