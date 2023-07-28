using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using System.Threading.Tasks;

namespace Esfa.Recruit.Employer.Web.Orchestrators
{
    public interface IApplicationReviewsOrchestrator
    {
        Task<ApplicationReviewsUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm);
        Task<ApplicationReviewsUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(ApplicationReviewsToUnsuccessfulConfirmationRouteModel rm);
    }

    public class ApplicationReviewsOrchestrator : IApplicationReviewsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;

        public ApplicationReviewsOrchestrator(IRecruitVacancyClient client)
        {
            _vacancyClient = client;
        }

        public async Task<ApplicationReviewsUnsuccessfulViewModel> GetApplicationReviewsToUnsuccessfulViewModelAsync(VacancyRouteModel rm)
        {
            var vacancy = await _vacancyClient.GetVacancyAsync(rm.VacancyId);

            var applicationReviews = await _vacancyClient.GetVacancyApplicationsAsync(vacancy.VacancyReference.Value);

            return new ApplicationReviewsUnsuccessfulViewModel
            {
                VacancyId = vacancy.Id,
                EmployerAccountId = vacancy.EmployerAccountId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = applicationReviews
            };
        }

        public async Task<ApplicationReviewsUnsuccessfulConfirmationViewModel> GetApplicationReviewsToUnsuccessfulConfirmationViewModelAsync(ApplicationReviewsToUnsuccessfulConfirmationRouteModel rm)
        {
            // todo

            return new ApplicationReviewsUnsuccessfulConfirmationViewModel
            {
            };
        }
    }
}
