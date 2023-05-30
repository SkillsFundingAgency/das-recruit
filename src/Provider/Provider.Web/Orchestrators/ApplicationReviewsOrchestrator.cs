using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ApplicationReviewsOrchestrator
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
                VacancyId = rm.VacancyId,
                VacancyReference = vacancy.VacancyReference.Value,
                VacancyApplications = applicationReviews
            };
        }
    }
}
