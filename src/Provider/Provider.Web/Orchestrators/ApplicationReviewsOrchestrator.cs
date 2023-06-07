using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Models.ApplicationReviews;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public interface IApplicationReviewsOrchestrator
    {
        Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareWithEmployerViewModelAsync(VacancyRouteModel rm);
        Task<ShareMultipleApplicationReviewsConfirmationViewModel> GetApplicationReviewsToShareConfirmationViewModel(ShareMultipleApplicationsRequest request);
    }

    public class ApplicationReviewsOrchestrator : IApplicationReviewsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewsOrchestrator(IRecruitVacancyClient client)
        {
            _vacancyClient = client;
        }

        public async Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareWithEmployerViewModelAsync(VacancyRouteModel rm)
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

        public async Task<ShareMultipleApplicationReviewsConfirmationViewModel> GetApplicationReviewsToShareConfirmationViewModel(ShareMultipleApplicationsRequest request)
        {
            var applicationReviewsToShare = await _vacancyClient.GetVacancyApplicationsForSelectedIdsAsync(request.ApplicationsToShare);

            return new ShareMultipleApplicationReviewsConfirmationViewModel
            {
                VacancyId = request.VacancyId,
                Ukprn = request.Ukprn,
                ApplicationReviewsToShare = applicationReviewsToShare
            };
        }
    }
}
