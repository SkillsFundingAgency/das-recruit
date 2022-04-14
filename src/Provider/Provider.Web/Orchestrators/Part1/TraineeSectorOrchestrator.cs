using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class TraineeSectorOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;
        private readonly IReviewSummaryService _reviewSummaryService;

        public TraineeSectorOrchestrator(IRecruitVacancyClient vacancyClient, IUtility utility,IReviewSummaryService reviewSummaryService)
        {
            _vacancyClient = vacancyClient;
            _utility = utility;
            _reviewSummaryService = reviewSummaryService;
        }

        public async Task<TraineeSectorViewModel> GetTraineeViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancyTask = _utility.GetAuthorisedVacancyForEditAsync(vrm, RouteNames.TraineeSector_Get);
            var routesTask = _vacancyClient.GetApprenticeshipRoutes();
            
            await Task.WhenAll(vacancyTask, routesTask);
            
            var vacancy = vacancyTask.Result;
            var routes = routesTask.Result;
            
            var vm = new TraineeSectorViewModel
            {
                Title = vacancy.Title,
                VacancyId = vacancy.Id,
                SelectedRouteId = vacancy.RouteId.GetValueOrDefault(),
                Routes = routes.Select(c=> new ApprenticeshipRouteViewModel
                {
                    Id = c.Id,
                    Name = c.Route
                }),
                PageInfo = _utility.GetPartOnePageInfo(vacancy),
                Ukprn = vrm.Ukprn
            };
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetTrainingReviewFieldIndicators());
            }

            return vm;
        }

        public async Task<TraineeSectorViewModel> PostTraineeEditModelAsync(TraineeSectorEditModel editModel)
        {
            return null;
        }
    }
}