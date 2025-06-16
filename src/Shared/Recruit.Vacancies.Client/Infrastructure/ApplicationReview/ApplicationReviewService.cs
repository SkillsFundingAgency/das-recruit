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

    public class ApplicationReviewService(IOuterApiClient outerApiClient) : IBaseApplicationReviewRepository
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

        public async Task UpdateApplicationReviewsAsync(IEnumerable<Guid> applicationReviewIds,
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


        public async Task UpdateApplicationReviewsPendingUnsuccessfulFeedback(long vacancyReference,
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
                        }))
                ).ToList();

            await Task.WhenAll(tasks);
        }
    }
}
