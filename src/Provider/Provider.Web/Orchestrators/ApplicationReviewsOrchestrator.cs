using System;
using System.Linq;
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
        Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user);
        Task PostApplicationReviewsStatus(ApplicationReviewsToUpdateStatusModel request, VacancyUser user, ApplicationReviewStatus? applicationReviewStatus, ApplicationReviewStatus? applicationReviewTemporaryStatus);
        Task PostApplicationReviewPendingUnsuccessfulFeedback(ApplicationReviewStatusModel request, VacancyUser user, ApplicationReviewStatus applicationReviewStatus);
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
            var applicationsToUnsuccessful =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(request.VacancyId!.Value!,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful);
            
            return new ApplicationReviewsToUnsuccessfulConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationsToUnsuccessful= applicationsToUnsuccessful,
                CandidateFeedback = applicationsToUnsuccessful.FirstOrDefault()!.CandidateFeedback
            };
        }

        public async Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference!.Value, sortColumn, sortOrder);
            var applicationsSelected =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId.GetValueOrDefault(),
                    ApplicationReviewStatus.PendingToMakeUnsuccessful);

            var vacancyApplications = applicationReviews.Where(fil => fil.IsNotWithdrawn).ToList();
            foreach (var application in applicationsSelected)
            {
                vacancyApplications.FirstOrDefault(c => c.ApplicationId == application.ApplicationId)!.Selected = true;
            }
            
            return new ApplicationReviewsToUnsuccessfulViewModel
            {
                VacancyId = vacancy.Id,
                Ukprn = rm.Ukprn,
                VacancyApplications = vacancyApplications
            };
        }

        public async Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder);
            var applicationsSelected =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId.GetValueOrDefault(),
                    ApplicationReviewStatus.PendingShared);
            var vacancyApplications = applicationReviews.Where(fil => fil.IsNotWithdrawn).ToList();
            foreach (var application in applicationsSelected)
            {
                if (vacancyApplications.FirstOrDefault(c => c.ApplicationId == application.ApplicationId) != null)
                {
                    vacancyApplications.FirstOrDefault(c => c.ApplicationId == application.ApplicationId)!.Selected = true;    
                }
            }

            return new ShareMultipleApplicationReviewsViewModel
            {
                VacancyId = vacancy.Id,
                Ukprn = rm.Ukprn,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = vacancyApplications
            };
        }

        public async Task<ShareMultipleApplicationReviewsConfirmationViewModel> GetApplicationReviewsToShareConfirmationViewModel(ShareApplicationReviewsRequest request)
        {
            var applicationReviewsToShare =
                await _vacancyClient.GetVacancyApplicationsForReferenceAndStatus(request.VacancyId!.Value!,
                    ApplicationReviewStatus.PendingShared);
            
            return new ShareMultipleApplicationReviewsConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationReviewsToShare = applicationReviewsToShare
            };
        }

        public async Task PostApplicationReviewsStatus(ApplicationReviewsToUpdateStatusModel request, VacancyUser user, ApplicationReviewStatus? applicationReviewStatus, ApplicationReviewStatus? applicationReviewTemporaryStatus)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(request.VacancyId);
            await _vacancyClient.SetApplicationReviewsStatus(vacancy!.VacancyReference!.Value, request.ApplicationReviewIds, user, applicationReviewStatus, request.VacancyId,applicationReviewTemporaryStatus);
        }

        public async Task PostApplicationReviewPendingUnsuccessfulFeedback(ApplicationReviewStatusModel request, VacancyUser user, ApplicationReviewStatus applicationReviewStatus)
        {
            await _vacancyClient.SetApplicationReviewsPendingUnsuccessfulFeedback(user, applicationReviewStatus, request.VacancyId, request.CandidateFeedback);
        }

        public async Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user)
        {
            await _vacancyClient.SetApplicationReviewsToUnsuccessful(request.ApplicationsToUnsuccessful.Select(c=>c.ApplicationReviewId), request.CandidateFeedback, user, request.VacancyId!.Value!);
        }
    }
}
