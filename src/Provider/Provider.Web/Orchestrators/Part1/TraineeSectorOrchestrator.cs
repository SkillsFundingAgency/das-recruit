using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part1.Training;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Vacancies.Client.Application.Validation;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part1
{
    public class TraineeSectorOrchestrator : VacancyValidatingOrchestrator<TraineeSectorEditModel>
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IUtility _utility;
        private readonly IReviewSummaryService _reviewSummaryService;
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.RouteId;

        public TraineeSectorOrchestrator(
            ILogger<TraineeSectorOrchestrator> logger,
            IRecruitVacancyClient vacancyClient,
            IUtility utility,
            IReviewSummaryService reviewSummaryService) : base(logger)
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
                    ReviewFieldMappingLookups.GetTraineeSectorFieldIndicators());
            }

            return vm;
        }

        public async Task<OrchestratorResponse> PostTraineeSectorEditModelAsync(TraineeSectorEditModel editModel, VacancyUser vacancyUser)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.TraineeSector_Post);

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.RouteId,
                FieldIdResolver.ToFieldId(v => v.RouteId),
                vacancy,
                v => { return v.RouteId = editModel.SelectedRouteId; });

            return await ValidateAndExecute(
                vacancy,
                v => _vacancyClient.Validate(v, ValidationRules),
                v => _vacancyClient.UpdateDraftVacancyAsync(vacancy, vacancyUser)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, TraineeSectorEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, TraineeSectorEditModel>
            {
                {vacancy => vacancy.RouteId, model => model.SelectedRouteId}
            };
            return mappings;
        }
    }
}