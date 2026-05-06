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

        Task<bool> IsAllApplicationReviewsHasOutcomeAsync(Guid? vacancyId);
    }

    public class ApplicationReviewsOrchestrator(IRecruitVacancyClient client) : IApplicationReviewsOrchestrator
    {
        public async Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModel(ApplicationReviewsToUnsuccessfulRouteModel request)
        {
            var applicationsToUnsuccessful =
                await client.GetVacancyApplicationsForReferenceAndStatus(request.VacancyId!.Value!,
                    ApplicationReviewStatus.PendingToMakeUnsuccessful);
            
            return new ApplicationReviewsToUnsuccessfulConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationsToUnsuccessful= applicationsToUnsuccessful,
                CandidateFeedback = applicationsToUnsuccessful.FirstOrDefault()!.CandidateFeedback,
                IsMultipleApplications = applicationsToUnsuccessful.Count > 1,
            };
        }

        public async Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await client.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await client.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference!.Value, sortColumn, sortOrder);
            var applicationsSelected =
                await client.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId.GetValueOrDefault(),
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
            var vacancy = await client.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await client.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder);
            var applicationsSelected =
                await client.GetVacancyApplicationsForReferenceAndStatus(rm.VacancyId.GetValueOrDefault(),
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
                await client.GetVacancyApplicationsForReferenceAndStatus(request.VacancyId!.Value!,
                    ApplicationReviewStatus.PendingShared);

            if (applicationReviewsToShare.Count == 0 && request.ApplicationsToShare?.Count == 1)
            {
                var vacancy = await client.GetVacancyAsync(request.VacancyId!.Value);
                await client.SetApplicationReviewsStatus(vacancy.VacancyReference!.GetValueOrDefault(), request.ApplicationsToShare, null, null, request.VacancyId!.Value, ApplicationReviewStatus.PendingShared);
                applicationReviewsToShare.AddRange(await client.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToShare));
            }
            
            return new ShareMultipleApplicationReviewsConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationReviewsToShare = applicationReviewsToShare
            };
        }

        public async Task PostApplicationReviewsStatus(ApplicationReviewsToUpdateStatusModel request, VacancyUser user, ApplicationReviewStatus? applicationReviewStatus, ApplicationReviewStatus? applicationReviewTemporaryStatus)
        {
            var vacancy = await client.GetVacancyAsync(request.VacancyId);
            await client.SetApplicationReviewsStatus(vacancy!.VacancyReference!.Value, request.ApplicationReviewIds, user, applicationReviewStatus, request.VacancyId, applicationReviewTemporaryStatus);
        }

        public async Task PostApplicationReviewPendingUnsuccessfulFeedback(ApplicationReviewStatusModel request, VacancyUser user, ApplicationReviewStatus applicationReviewStatus)
        {
            await client.SetApplicationReviewsPendingUnsuccessfulFeedback(user, applicationReviewStatus, request.VacancyId, request.CandidateFeedback);
        }

        public async Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user)
        {
            await client.SetApplicationReviewsToUnsuccessful(request.ApplicationsToUnsuccessful.Select(c=>c.ApplicationReviewId), request.CandidateFeedback, user, request.VacancyId!.Value!);
        }

        public async Task<bool> IsAllApplicationReviewsHasOutcomeAsync(Guid? vacancyId)
        {
            if (!vacancyId.HasValue)
                return false;

            var applicationReviews = await client.GetApplicationReviewsAsync(vacancyId.Value);
            return applicationReviews
                .Where(ar => !ar.IsWithdrawn)
                .All(ar => ar.Status is ApplicationReviewStatus.Successful or ApplicationReviewStatus.Unsuccessful);
        }
    }
}
