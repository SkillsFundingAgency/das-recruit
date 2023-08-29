using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IApplicationReviewsOrchestrator
    {
        Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder);
        Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(ApplicationReviewsToUnsuccessfulRouteModel rm);
        ApplicationReviewsFeedbackViewModel GetApplicationReviewsFeedbackViewModel(ApplicationReviewsToUnsuccessfulRouteModel rm);
        Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user);
    }

    public class ApplicationReviewsOrchestrator : IApplicationReviewsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewsOrchestrator(IRecruitVacancyClient client)
        {
            _vacancyClient = client;
        }

        public async Task<ApplicationReviewsToUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm, SortColumn sortColumn, SortOrder sortOrder)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId);

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsSortedAsync(vacancy.VacancyReference.Value, sortColumn, sortOrder);

            return new ApplicationReviewsToUnsuccessfulViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = applicationReviews
            };
        }
        public ApplicationReviewsFeedbackViewModel GetApplicationReviewsFeedbackViewModel(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            return new ApplicationReviewsFeedbackViewModel
            {
                VacancyId = rm.VacancyId,
                EmployerAccountId = rm.EmployerAccountId,
                ApplicationsToUnsuccessful = rm.ApplicationsToUnsuccessful,
                Outcome = ApplicationReviewStatus.Unsuccessful
            };
        }

        public async Task<ApplicationReviewsToUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(ApplicationReviewsToUnsuccessfulRouteModel rm)
        {
            var vacancyApplicationsToUnsuccessful = await _vacancyClient.GetVacancyApplicationsForSelectedIdsAsync(rm.ApplicationsToUnsuccessful);

            return new ApplicationReviewsToUnsuccessfulConfirmationViewModel
            {
                VacancyId = rm.VacancyId,
                EmployerAccountId = rm.EmployerAccountId,
                VacancyApplicationsToUnsuccessful = vacancyApplicationsToUnsuccessful,
                CandidateFeedback = rm.CandidateFeedback
            };
        }

        public async Task PostApplicationReviewsToUnsuccessfulAsync(ApplicationReviewsToUnsuccessfulConfirmationViewModel request, VacancyUser user)
        {
            await _vacancyClient.SetApplicationReviewsToUnsuccessful(request.VacancyApplicationsToUnsuccessful, request.CandidateFeedback, user);
        }
    }
}
