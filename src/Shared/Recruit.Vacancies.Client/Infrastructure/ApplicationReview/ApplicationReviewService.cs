using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Repositories;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Requests;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ApplicationReview.Responses;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.OuterApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Address = Esfa.Recruit.Vacancies.Client.Domain.Entities.Address;

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

    public class ApplicationReviewRepositoryRunner(IEnumerable<IApplicationWriteRepository> applicationReviewResolver)
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

    public class ApplicationReviewService(IOuterApiClient outerApiClient, ILogger<ApplicationReviewService> logger) : IApplicationWriteRepository, ISqlDbRepository
    {
        public async Task UpdateAsync(Domain.Entities.ApplicationReview applicationReview)
        {
            await outerApiClient.Post(new PostApplicationReviewApiRequest(applicationReview.Id,
                new PostApplicationReviewApiRequestData
                {
                    Status = applicationReview.Status.ToString(),
                    DateSharedWithEmployer = applicationReview.DateSharedWithEmployer,
                    HasEverBeenEmployerInterviewing = applicationReview.HasEverBeenEmployerInterviewing ?? false,
                    TemporaryReviewStatus = applicationReview.TemporaryReviewStatus.ToString(),
                    CandidateFeedback = applicationReview.CandidateFeedback,
                    EmployerFeedback = applicationReview.EmployerFeedback
                }), false);
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
                        CandidateFeedback = candidateFeedback
                        // CandidateFeedback and VacancyReference can be added to PostApplicationReviewApiRequestData if supported
                    }), false)
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
                        }), false)
                ).ToList();

            await Task.WhenAll(tasks);
        }

        public Task CreateAsync(Domain.Entities.ApplicationReview review)
        {
            throw new NotImplementedException();
        }

        public async Task<Domain.Entities.ApplicationReview> GetAsync(Guid applicationReviewId)
        {
            var response = await outerApiClient.Get<GetApplicationReviewByIdApiResponse>(
                new GetApplicationReviewByIdApiRequest(applicationReviewId));

            return response?.ApplicationReview == null
                ? null
                : MapToDomainApplicationReview(response.ApplicationReview);
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

        public async Task<List<T>> GetForVacancyAsync<T>(long vacancyReference)
        {
            var response = await outerApiClient.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                new GetApplicationReviewsByVacancyReferenceApiRequest(vacancyReference));

            if (response?.ApplicationReviews == null || response.ApplicationReviews.Count == 0) return [];

            var applicationReviews = response.ApplicationReviews
                .Where(fil => fil.DateSharedWithEmployer != null)
                .Select(MapToDomainApplicationReview).ToList();

            return applicationReviews.Cast<T>().ToList();
        }

        public async Task<List<Domain.Entities.ApplicationReview>> GetForVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder)
        {
            var response = await outerApiClient.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                new GetApplicationReviewsByVacancyReferenceApiRequest(vacancyReference));

            if (response?.ApplicationReviews == null || response.ApplicationReviews.Count == 0) return [];

            var applicationReviews = response.ApplicationReviews
                .Select(MapToDomainApplicationReview).ToList();

            var sortedResult = applicationReviews.AsQueryable()
                .Sort(sortColumn, sortOrder);

            return sortedResult.ToList();
        }

        public async Task<List<Domain.Entities.ApplicationReview>> GetForSharedVacancyAsync(long vacancyReference)
        {
            var response = await outerApiClient.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                new GetApplicationReviewsByVacancyReferenceApiRequest(vacancyReference));

            if (response?.ApplicationReviews == null || response.ApplicationReviews.Count == 0) return [];

            var applicationReviews = response.ApplicationReviews
                .Where(fil => fil.DateSharedWithEmployer != null)
                .Select(MapToDomainApplicationReview).ToList();

            return applicationReviews.ToList();
        }

        public async Task<List<Domain.Entities.ApplicationReview>> GetForSharedVacancySortedAsync(long vacancyReference, SortColumn sortColumn, SortOrder sortOrder)
        {
            var response = await outerApiClient.Get<GetApplicationReviewsByVacancyReferenceApiResponse>(
                new GetApplicationReviewsByVacancyReferenceApiRequest(vacancyReference));

            if (response?.ApplicationReviews == null || response.ApplicationReviews.Count == 0) return [];

            var applicationReviews = response.ApplicationReviews
                .Where(fil => fil.DateSharedWithEmployer != null)
                .Select(MapToDomainApplicationReview).ToList();

            var sortedResult = applicationReviews.AsQueryable()
                .Sort(sortColumn, sortOrder);

            return sortedResult.ToList();
        }

        public Task<List<T>> GetAllForSelectedIdsAsync<T>(List<Guid> applicationReviewIds)
        {
            throw new NotImplementedException();
        }

        public Task<List<Domain.Entities.ApplicationReview>> GetAllForVacancyWithTemporaryStatus(long vacancyReference, ApplicationReviewStatus status)
        {
            throw new NotImplementedException();
        }

        private string GetCandidateAppliedLocation(List<Responses.Address> addresses)
        {
            if (addresses == null || addresses.Count == 0)
            {
                return null;
            }

            // Filter selected addresses and deserialize them
            var selectedAddresses = addresses
                .Where(x => x.IsSelected)
                .Select(x =>
                {
                    Address employmentAddress;
                    try
                    {
                        employmentAddress = !string.IsNullOrWhiteSpace(x.FullAddress)
                            ? JsonConvert.DeserializeObject<Address>(x.FullAddress)
                            : null;
                    }
                    catch (JsonException)
                    {
                        // If deserialization fails, we set employmentAddress to null
                        logger.LogWarning("Failed to deserialize address:{Address}", x.FullAddress);
                        employmentAddress = null;
                    }

                    return employmentAddress;
                })
                .ToList();

            return selectedAddresses.GetCities();
        }

        private Domain.Entities.ApplicationReview MapToDomainApplicationReview(Responses.ApplicationReview response)
        {
            return new Domain.Entities.ApplicationReview
            {
                Id = response.Id,
                CandidateId = response.CandidateId,
                VacancyReference = response.VacancyReference,
                Status = Enum.Parse<ApplicationReviewStatus>(response.Status),
                TemporaryReviewStatus = response.TemporaryReviewStatus != null
                    ? Enum.Parse<ApplicationReviewStatus>(response.TemporaryReviewStatus)
                    : null,
                CreatedDate = response.CreatedDate,
                DateSharedWithEmployer = response.DateSharedWithEmployer,
                ReviewedDate = response.ReviewedDate,
                SubmittedDate = response.SubmittedDate,
                WithdrawnDate = response.WithdrawnDate,
                CandidateFeedback = response.CandidateFeedback,
                EmployerFeedback = response.EmployerFeedback,
                VacancyTitle = response.VacancyTitle,
                HasEverBeenEmployerInterviewing = response.HasEverBeenEmployerInterviewing,
                IsWithdrawn = response.WithdrawnDate != null,
                AdditionalQuestion1 = response.AdditionalQuestion1,
                AdditionalQuestion2 = response.AdditionalQuestion2,
                Application = response.Application != null ? new Domain.Entities.Application
                {
                    ApplicationId = response.Application.Id,
                    CandidateId = response.Application.CandidateId,
                    FirstName = response.Application.Candidate?.FirstName,
                    LastName = response.Application.Candidate?.LastName,
                    CandidateAppliedLocations = GetCandidateAppliedLocation(response.Application?.EmploymentLocation?.Addresses)
                } : null
            };
        }
    }
}