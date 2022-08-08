using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Interfaces;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.FutureProspects;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class FutureProspectsOrchestrator : VacancyValidatingOrchestrator<FutureProspectsEditModel>, IFutureProspectsOrchestrator
    {
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;

        public FutureProspectsOrchestrator(IRecruitVacancyClient vacancyClient,
            ILogger<FutureProspectsOrchestrator> logger,
            IReviewSummaryService reviewSummaryService,
            IUtility utility) : base(logger)
        {
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }
        
        public async Task<FutureProspectsViewModel> GetViewModel(VacancyRouteModel routeModel)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(routeModel, RouteNames.FutureProspects_Get);

            var viewModel = new FutureProspectsViewModel
            {
                VacancyId = vacancy.Id,
                Title = vacancy.Title,
                FutureProspects = vacancy.OutcomeDescription
            };
            
            if (vacancy.Status == VacancyStatus.Referred)
            {
                viewModel.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value, 
                    ReviewFieldMappingLookups.GetFutureProspectsFieldIndicators());
            }
            
            viewModel.IsTaskListCompleted = _utility.IsTaskListCompleted(vacancy);

            return viewModel;
        }

        public async Task<OrchestratorResponse> PostEditModel(FutureProspectsEditModel futureProspectsEditModel, VacancyUser toVacancyUser)
        {
            throw new System.NotImplementedException();
        }
        
        protected override EntityToViewModelPropertyMappings<Vacancy, FutureProspectsEditModel> DefineMappings()
        {
            throw new System.NotImplementedException();
        }
    }
}