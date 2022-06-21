using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.Mappings;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.WorkExperience;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class WorkExperienceOrchestrator : VacancyValidatingOrchestrator<WorkExperienceEditModel>
    {
        private readonly IRecruitVacancyClient _recruitVacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly IUtility _utility;
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.WorkExperience;

        public WorkExperienceOrchestrator(
            IRecruitVacancyClient recruitVacancyClient, 
            ILogger<WorkExperienceOrchestrator> logger, 
            IReviewSummaryService reviewSummaryService, 
            IUtility utility) : base(logger)
        {
            _recruitVacancyClient = recruitVacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _utility = utility;
        }

        public async Task<WorkExperienceViewModel> GetWorkExperienceViewModelAsync(VacancyRouteModel vacancyRouteModel)
        {
            var vacancy =
                await _utility.GetAuthorisedVacancyForEditAsync(vacancyRouteModel, RouteNames.WorkExperience_Get);

            var viewModel = new WorkExperienceViewModel
            {
                Title = vacancy.Title,
                WorkExperience = vacancy.WorkExperience,
                Ukprn = vacancyRouteModel.Ukprn,
                VacancyId = vacancyRouteModel.VacancyId
            };

            if (vacancy.Status == VacancyStatus.Referred)
            {
                viewModel.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference!.Value,
                    ReviewFieldMappingLookups.GetWorkExperienceFieldIndicators());
            }
            
            viewModel.IsTaskListCompleted = _utility.TaskListCompleted(vacancy);

            return viewModel;
        }
        
        public async Task<WorkExperienceViewModel> GetWorkExperienceViewModelAsync(WorkExperienceEditModel m)
        {
            var vm = await GetWorkExperienceViewModelAsync((VacancyRouteModel)m);

            vm.WorkExperience = m.WorkExperience;

            return vm;
        }

        public async Task<OrchestratorResponse> PostWorkExperienceEditModelAsync(WorkExperienceEditModel editModel, VacancyUser vacancyUser)
        {
            var vacancy = await _utility.GetAuthorisedVacancyForEditAsync(editModel, RouteNames.FutureProspects_Post);

            SetVacancyWithProviderReviewFieldIndicators(
                vacancy.WorkExperience,
                FieldIdResolver.ToFieldId(v => v.WorkExperience),
                vacancy,
                (v) => { return v.WorkExperience = editModel.WorkExperience; });

            return await ValidateAndExecute(
                vacancy,
                v => _recruitVacancyClient.Validate(v, ValidationRules),
                v => _recruitVacancyClient.UpdateDraftVacancyAsync(vacancy, vacancyUser)
            );
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, WorkExperienceEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, WorkExperienceEditModel>
            {
                {vacancy => vacancy.WorkExperience, model => model.WorkExperience}
            };
            return mappings;
        }
    }
}