using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Provider.Web.Configuration.Routing;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Provider.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Shared.Web.Extensions;
using Esfa.Recruit.Shared.Web.Orchestrators;
using Esfa.Recruit.Shared.Web.Services;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Provider.Web.Orchestrators.Part2
{
    public class SkillsOrchestrator : EntityValidatingOrchestrator<Vacancy, SkillsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Skills;
        private readonly IProviderVacancyClient _client;
        private readonly IRecruitVacancyClient _vacancyClient;
        private readonly IReviewSummaryService _reviewSummaryService;
        private readonly SkillsViewModelHelper _skillsOrchestratorHelper;

        public SkillsOrchestrator(IProviderVacancyClient client, IRecruitVacancyClient vacancyClient, ILogger<SkillsOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _vacancyClient = vacancyClient;
            _reviewSummaryService = reviewSummaryService;
            _skillsOrchestratorHelper = new SkillsViewModelHelper(vacancyClient);
        }
        
        public async Task<SkillsViewModel> GetSkillsViewModelAsync(VacancyRouteModel vrm, string[] draftSkills = null)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, vrm, RouteNames.Skills_Get);

            var vm = new SkillsViewModel
            {
                Title = vacancy.Title
            };

            if (draftSkills == null)
            {
                _skillsOrchestratorHelper.SetViewModelSkillsFromVacancy(vm, vacancy);
            }
            else
            {
                _skillsOrchestratorHelper.SetViewModelSkillsFromDraftSkills(vm, draftSkills);
            }

            if (vacancy.Status == VacancyStatus.Referred)
            {
                //vm.Review = await _reviewSummaryService.GetReviewSummaryViewModelAsync(vacancy.VacancyReference.Value,
                //    ReviewFieldMappingLookups.GetSkillsFieldIndicators());
            }

            return vm;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(SkillsEditModel m)
        {
            var vm = await GetSkillsViewModelAsync((VacancyRouteModel)m);

            _skillsOrchestratorHelper.SetViewModelSkillsFromDraftSkills(vm, m.Skills);

            vm.AddCustomSkillName = m.AddCustomSkillName;
            
            return vm;
        }

        public async Task<OrchestratorResponse> PostSkillsEditModelAsync(SkillsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, _vacancyClient, m, RouteNames.Skills_Post);

            _skillsOrchestratorHelper.SetVacancyFromEditModel(vacancy, m);

            var isAddingCustomSkill = !string.IsNullOrEmpty(m.AddCustomSkill);
            var isRemovingCustomSkill = !string.IsNullOrWhiteSpace(m.RemoveCustomSkill);

            //if we are adding/removing a skill then just validate and don't persist
            var validateOnly = isAddingCustomSkill || isRemovingCustomSkill;

            return await ValidateAndExecute(vacancy,
                v =>
                {
                    var result = _vacancyClient.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors, m, vacancy);
                    return result;
                },
                v => validateOnly ? Task.CompletedTask : _vacancyClient.UpdateDraftVacancyAsync(v, user));
        }
        
        protected override EntityToViewModelPropertyMappings<Vacancy, SkillsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, SkillsEditModel>
            {
                { e => e.Skills, vm => vm.Skills }
            };

            return mappings;
        }

        private void SyncErrorsAndModel(ICollection<EntityValidationError> errors, SkillsEditModel m, Vacancy vacancy)
        {
            const string skillsPropertyName = nameof(Vacancy.Skills);

            //Get the first invalid skill
            var skillError = errors.FirstOrDefault(e => e.PropertyName.StartsWith($"{skillsPropertyName}["));
            if (skillError == null)
            {
                return;
            }

            //Populate AddCustomSkillName so we can edit the invalid skill
            var skillIndex = skillError.GetIndexPosition().Value;
            var invalidSkill = vacancy.Skills[skillIndex];
            m.AddCustomSkillName = invalidSkill;
            
            // Remove from vacancy and view model lists
            m.Skills.RemoveAt(skillIndex);
            vacancy.Skills.RemoveAt(skillIndex);

            //Attach the error to AddCustomSkillName
            skillError.PropertyName = nameof(m.AddCustomSkillName);

            //Remove other skill errors
            errors.Where(e => e.PropertyName.StartsWith($"{skillsPropertyName}[")).ToList()
                .ForEach(r => errors.Remove(r));
        }
    }
}