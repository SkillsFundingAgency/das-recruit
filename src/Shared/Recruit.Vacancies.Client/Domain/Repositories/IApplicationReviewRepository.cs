using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
using ApplicationReview = Esfa.Recruit.Vacancies.Client.Domain.Entities.ApplicationReview;

namespace Esfa.Recruit.Vacancies.Client.Domain.Repositories
{
    public interface IApplicationReadRepository
    {
        Task<ApplicationReview> GetAsync(Guid applicationReviewId);
        Task<List<T>> GetForVacancyAsync<T>(long vacancyReference);
        Task<List<ApplicationReview>> GetForSharedVacancyAsync(long vacancyReference);
        Task<List<ApplicationReview>> GetForVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder);
        Task<List<ApplicationReview>> GetForSharedVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder);
        Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId);
        Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds);
        Task<List<ApplicationReview>> GetAllForVacancyWithTemporaryStatus(long vacancyReference, ApplicationReviewStatus status);
        Task<GetApplicationReviewsCountByVacancyReferenceApiResponse> GetApplicationReviewsCountByVacancyReferenceAsync(
            long vacancyReference);
    }

    public interface IMongoDbRepository : IApplicationReadRepository;
    public interface ISqlDbRepository : IApplicationReadRepository;

    public interface IApplicationReviewRepository : IMongoDbRepository, ISqlDbRepository
    {
        Task CreateAsync(ApplicationReview review);
        Task<ApplicationReview> GetAsync(long vacancyReference, Guid candidateId);
        Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds);
        Task<List<ApplicationReview>> GetAllForVacancyWithTemporaryStatus(long vacancyReference, ApplicationReviewStatus status);
    }

    public interface IApplicationWriteRepository
    {
        Task UpdateAsync(ApplicationReview applicationReview);
        Task UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds, VacancyUser user, DateTime updatedDate, ApplicationReviewStatus? status, ApplicationReviewStatus? temporaryReviewStatus, string candidateFeedback = null, long? vacancyReference = null);
        Task UpdateApplicationReviewsPendingUnsuccessfulFeedback(long vacancyReference, VacancyUser user, DateTime updatedDate, string candidateFeedback);
    }
}