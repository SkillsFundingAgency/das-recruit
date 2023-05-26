using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReviews;

namespace Esfa.Recruit.Provider.Web.Orchestrators
{
    public class ApplicationReviewsOrchestrator
    {
        public ApplicationReviewsOrchestrator()
        {
        }

        public async Task<ShareMultipleApplicationReviewsViewModel> GetApplicationReviewsToShareWithEmployerViewModelAsync(VacancyRouteModel rm)
        {
            return new ShareMultipleApplicationReviewsViewModel
            {
                VacancyId = rm.VacancyId
            };
        }
    }
}
