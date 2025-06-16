using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview
{
    public interface IApplicationReviewRepositoryRunner
    {
        Task UpdateAsync(Domain.Entities.ApplicationReview applicationReview);

        Task UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds,
            VacancyUser user,
            DateTime updatedDate,
            ApplicationReviewStatus? status,
            ApplicationReviewStatus? temporaryReviewStatus,
            string candidateFeedback = null,
            long? vacancyReference = null);

        Task UpdateApplicationReviewsPendingUnsuccessfulFeedback(long vacancyReference,
            VacancyUser user,
            DateTime updatedDate,
            string candidateFeedback);
    }

    public class ApplicationReviewRepositoryRunner(IEnumerable<IApplicationReviewRepository> applicationReviewResolver)
        : IApplicationReviewRepositoryRunner
    {
        public async Task UpdateAsync(Domain.Entities.ApplicationReview applicationReview)
        {
            foreach (var applicationReviewRepository in applicationReviewResolver)
            {
                await applicationReviewRepository.UpdateAsync(applicationReview);
            }
        }

        public async Task UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds, VacancyUser user,
            DateTime updatedDate,
            ApplicationReviewStatus? status, ApplicationReviewStatus? temporaryReviewStatus,
            string candidateFeedback = null,
            long? vacancyReference = null)
        {
            foreach (var applicationReviewRepository in applicationReviewResolver)
            {
                await applicationReviewRepository.UpdateApplicationReviewsAsync(applicationReviewIds, user, updatedDate,
                    status, temporaryReviewStatus, candidateFeedback, vacancyReference);
            }
        }

        public async Task UpdateApplicationReviewsPendingUnsuccessfulFeedback(long vacancyReference, VacancyUser user,
            DateTime updatedDate,
            string candidateFeedback)
        {
            foreach (var applicationReviewRepository in applicationReviewResolver)
            {
                await applicationReviewRepository.UpdateApplicationReviewsPendingUnsuccessfulFeedback(vacancyReference,
                    user, updatedDate, candidateFeedback);
            }
        }
    }

    public class ApplicationReviewService(IOuterApiClient outerApiClient) : IApplicationReviewRepository
    {
        public async Task UpdateAsync(Domain.Entities.ApplicationReview applicationReview)
        {
            await outerApiClient.Post(new PostApplicationReviewApiRequest(applicationReview.Id,
                new PostApplicationReviewApiRequestData
                {
                    Status = applicationReview.Status.ToString(),
                    DateSharedWithEmployer = applicationReview.DateSharedWithEmployer,
                    HasEverBeenEmployerInterviewing = applicationReview.HasEverBeenEmployerInterviewing ?? false,
                    TemporaryReviewStatus = applicationReview.TemporaryReviewStatus.ToString()
                }));
        }

        public async Task UpdateApplicationReviewsAsync(
            IEnumerable<Guid> applicationReviewIds,
            VacancyUser user,
            DateTime updatedDate,
            ApplicationReviewStatus? status,
            ApplicationReviewStatus? temporaryReviewStatus,
            string candidateFeedback = null,
            long? vacancyReference = null)
        {
            var tasks = applicationReviewIds.Select(applicationReviewId =>
                outerApiClient.Post(new PostApplicationReviewApiRequest(applicationReviewId,
                    new PostApplicationReviewApiRequestData
                    {
                        Status = status?.ToString(),
                        DateSharedWithEmployer = status == ApplicationReviewStatus.Shared 
                            ? updatedDate
                            : null,
                        TemporaryReviewStatus = temporaryReviewStatus?.ToString(),
                        // CandidateFeedback and VacancyReference can be added to PostApplicationReviewApiRequestData if supported
                    }))
            ).ToList();

            await Task.WhenAll(tasks);
        }


        public async Task UpdateApplicationReviewsPendingUnsuccessfulFeedback(
            long vacancyReference,
            VacancyUser user,
            DateTime updatedDate,
            string candidateFeedback)
        {
            var response = await outerApiClient.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                new GetApplicationReviewsByVacancyReferenceApiRequest(vacancyReference));

            if (response?.ApplicationReviews == null || response.ApplicationReviews.Count == 0)
            {
                return;
            }

            var tasks = response.ApplicationReviews
                .Where(fil => fil.WithdrawnDate == null && fil.TemporaryReviewStatus == ApplicationReviewStatus.PendingToMakeUnsuccessful.ToString())
                .Select(applicationReview =>
                    outerApiClient.Post(new PostApplicationReviewApiRequest(applicationReview.Id,
                        new PostApplicationReviewApiRequestData
                        {
                            Status = ApplicationReviewStatus.PendingToMakeUnsuccessful.ToString(),
                            DateSharedWithEmployer = updatedDate,
                            CandidateFeedback = candidateFeedback
                        }))
                ).ToList();

            await Task.WhenAll(tasks);
        }

        public Task CreateAsync(Domain.Entities.ApplicationReview review)
        {
            throw new NotImplementedException();
        }

        public Task<Domain.Entities.ApplicationReview> GetAsync(Guid applicationReviewId)
        {
            throw new NotImplementedException();
        }

        public Task<Domain.Entities.ApplicationReview> GetAsync(long vacancyReference, Guid candidateId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.ApplicationReview>> GetByStatusAsync(long vacancyReference, ApplicationReviewStatus status)
        {
            throw new NotImplementedException();
        }

        public Task HardDelete(Guid applicationReviewId)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetForVacancyAsync<T>(long vacancyReference)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.ApplicationReview>> GetForVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.ApplicationReview>> GetForSharedVacancyAsync(long vacancyReference)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.ApplicationReview>> GetForSharedVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder)
        {
            throw new NotImplementedException();
        }

        public Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.ApplicationReview>> GetAllForVacancyWithTemporaryStatus(long vacancyReference, ApplicationReviewStatus status)
        {
            throw new NotImplementedException();
        }
    }
}
