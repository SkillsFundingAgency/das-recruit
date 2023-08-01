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
        Task<ApplicationReviewsToUnSuccessfulConfirmationViewModel> GetApplicationReviewsToUnSuccessfulConfirmationViewModel(ApplicationReviewsStatusChangeModel request);
        Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm);
        Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareViewModelAsync(VacancyRouteModel rm);
        Task<ShareMultipleApplicationReviewsConfirmationViewModel> GetApplicationReviewsToShareConfirmationViewModel(ShareApplicationReviewsRequest request);
        Task PostApplicationReviewsStatusConfirmationAsync(ShareApplicationReviewsPostRequest request, VacancyUser user);
        Task PostApplicationReviewsToUnSuccessfulStatusConfirmationAsync(ApplicationReviewsToUnSuccessfulConfirmationViewModel request, VacancyUser user);
    }

    public class ApplicationReviewsOrchestrator : IApplicationReviewsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewsOrchestrator(IRecruitVacancyClient client)
        {
            _vacancyClient = client;
        }

        public async Task<ApplicationReviewsToUnSuccessfulConfirmationViewModel> GetApplicationReviewsToUnSuccessfulConfirmationViewModel(ApplicationReviewsStatusChangeModel request)
        {
            var applicationsToUnSuccessful = await _vacancyClient.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToUnSuccessful);

            return new ApplicationReviewsToUnSuccessfulConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationsToUnSuccessful = applicationsToUnSuccessful,
                CandidateFeedback = request.CandidateFeedback
            };
        }

        public async Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value);

            return new ApplicationReviewsToUnsuccessfulViewModel
            {
                VacancyId = vacancy.Id,
                Ukprn = rm.Ukprn,
                VacancyApplications = applicationReviews
            };
        }

        public async Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareViewModelAsync(VacancyRouteModel rm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId.GetValueOrDefault());

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value);

            return new ShareMultipleApplicationReviewsViewModel
            {
                VacancyId = vacancy.Id,
                Ukprn = rm.Ukprn,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = applicationReviews
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

        public async Task PostApplicationReviewsStatusConfirmationAsync(ShareApplicationReviewsPostRequest request, VacancyUser user)
        {
            await _vacancyClient.SetApplicationReviewsShared(request.ApplicationReviewsToShare, user);
        }

        public async Task PostApplicationReviewsToUnSuccessfulStatusConfirmationAsync(ApplicationReviewsToUnSuccessfulConfirmationViewModel request, VacancyUser user)
        {
            await _vacancyClient.SetApplicationReviewsToUnsuccessful(request.ApplicationsToUnSuccessful, request.CandidateFeedback, user);
        }

    }
}
