﻿using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public interface IApplicationReviewsOrchestrator
    {
        Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModel(ApplicationReviewsToUnsuccessfulRouteModel request);
        Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder);
        Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder);
        Task<ShareMultipleApplicationReviewsConfirmationViewModel> GetApplicationReviewsToShareConfirmationViewModel(ShareApplicationReviewsRequest request);
        Task PostApplicationReviewsToSharedAsync(ShareApplicationReviewsPostRequest request, VacancyUser user);
        Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user);
    }

    public class ApplicationReviewsOrchestrator : IApplicationReviewsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewsOrchestrator(IRecruitVacancyClient client)
        {
            _vacancyClient = client;
        }

        public async Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModel(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var applicationsToUnsuccessful = await _vacancyClient.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToUnsuccessful);

            return new ApplicationReviewsToUnsuccessfulConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationsToUnsuccessful= applicationsToUnsuccessful,
                CandidateFeedback = request.CandidateFeedback
            };
        }

        public async Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder);

            return new ApplicationReviewsToUnsuccessfulViewModel
            {
                VacancyId = vacancy.Id,
                Ukprn = rm.Ukprn,
                VacancyApplications = applicationReviews.Where(fil => fil.IsNotWithdrawn).ToList()
            };
        }

        public async Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder);

            return new ShareMultipleApplicationReviewsViewModel
            {
                VacancyId = vacancy.Id,
                Ukprn = rm.Ukprn,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = applicationReviews.Where(fil => fil.IsNotWithdrawn).ToList()
            };
        }

        public async Task<ShareMultipleApplicationReviewsConfirmationViewModel> GetApplicationReviewsToShareConfirmationViewModel(ShareApplicationReviewsRequest request)
        {
            var applicationReviewsToShare = await _vacancyClient.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToShare);

            return new ShareMultipleApplicationReviewsConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationReviewsToShare = applicationReviewsToShare
            };
        }

        public async Task PostApplicationReviewsToSharedAsync(ShareApplicationReviewsPostRequest request, VacancyUser user)
        {
            await _vacancyClient.SetApplicationReviewsShared(request.ApplicationReviewsToShare, user);
        }

        public async Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user)
        {
            await _vacancyClient.SetApplicationReviewsToUnsuccessful(request.ApplicationsToUnsuccessful, request.CandidateFeedback, user);
        }
    }
}
