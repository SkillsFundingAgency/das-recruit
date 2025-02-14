using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using MongoDB.Driver;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IApplicationReviewRepository
    {
        Task CreateAsync(ApplicationReview review);
        Task<ApplicationReview> GetAsync(Guid applicationReviewId);
        Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId);
        Task<List<ApplicationReview>> GetByStatusAsync(long vacancyReference, ApplicationReviewStatus status);
        Task UpdateAsync(ApplicationReview applicationReview);
        Task HardDelete(Guid applicationReviewId);
        Task<List<T>> GetForVacancyAsync<T>(long vacancyReference);
        Task<List<ApplicationReview>> GetForVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder);
        Task<List<ApplicationReview>> GetForSharedVacancyAsync(long vacancyReference);
        Task<List<ApplicationReview>> GetForSharedVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder);
        Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds);
        Task<List<ApplicationReview>> GetAllForVacancyWithTemporaryStatus(long vacancyReference, ApplicationReviewStatus status);
        Task<UpdateResult> UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds, VacancyUser user, DateTime updatedDate, ApplicationReviewStatus? status, ApplicationReviewStatus? temporaryReviewStatus, string candidateFeedback = null, long? vacancyReference = null);
        Task<UpdateResult> UpdateApplicationReviewsPendingUnsuccessfulFeedback(long vacancyReference, VacancyUser user, DateTime updatedDate, string candidateFeedback);
    }
}
