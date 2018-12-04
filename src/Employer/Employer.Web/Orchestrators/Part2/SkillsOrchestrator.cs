using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.Mappings;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Esfa.Recruit.Shared.Web.Orchestrators;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class SkillsOrchestrator : EntityValidatingOrchestrator<Vacancy, SkillsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Skills;
        private const int ColumnOneCutOffIndex = 9;
        private const char SortPrefixSeparator = '-';
        private readonly IEmployerVacancyClient _client;
        private readonly Lazy<List<string>> _lazyCandidateSkills;
        private readonly IReviewSummaryService _reviewSummaryService;

        private List<string> CandidateSkills => _lazyCandidateSkills.Value;
        private IEnumerable<string> Column1BuiltInSkills => CandidateSkills.Take(ColumnOneCutOffIndex);
        private IEnumerable<string> Column2BuiltInSkills => CandidateSkills.Skip(ColumnOneCutOffIndex);

        public SkillsOrchestrator(IEmployerVacancyClient client, ILogger<SkillsOrchestrator> logger, IReviewSummaryService reviewSummaryService) : base(logger)
        {
            _client = client;
            _lazyCandidateSkills = new Lazy<List<string>>(() => _client.GetCandidateSkillsAsync().Result);
            _reviewSummaryService = reviewSummaryService;
        }
        
        public async Task<SkillsViewModel> GetSkillsViewModelAsync(VacancyRouteModel vrm, string[] draftSkills = null)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.Skills_Get);

            var vm = new SkillsViewModel
            {
                Title = vacancy.Title
            };

            if (draftSkills == null)
            {
                SetViewModelSkillsFromVacancy(vm, vacancy);
            }
            else
            {
                SetViewModelSkillsFromDraftSkills(vm, draftSkills);
            }

            if (vacancy.Status == VacancyStatus.Referred)
            {
                vm.Review = await _reviewSummaryService.GetReviewSummaryViewModel(vacancy.VacancyReference.Value,
                    ReviewFieldMappingLookups.GetSkillsFieldIndicators());
            }

            return vm;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(SkillsEditModel m)
        {
            var vm = await GetSkillsViewModelAsync((VacancyRouteModel)m);

            SetViewModelSkillsFromDraftSkills(vm, m.Skills);

            vm.AddCustomSkillName = m.AddCustomSkillName;
            
            return vm;
        }

        private void SetViewModelSkillsFromVacancy(SkillsViewModel vm, Vacancy vacancy)
        {
            var orderedCustomSkills = GetCustomSkills(vacancy.Skills).ToArray();
            var baseSkills = GetBaseSkills(vacancy.Skills).ToArray();

            SetViewModelSkills(vm, baseSkills, orderedCustomSkills);
        }

        private void SetViewModelSkillsFromDraftSkills(SkillsViewModel vm, IList<string> draftSkills)
        {
            var orderedCustomSkills = ExtractAndSort(GetCustomSkills(draftSkills).ToArray());
            var baseSkills = GetBaseSkills(draftSkills).ToArray();

            SetViewModelSkills(vm, baseSkills, orderedCustomSkills);
        }

        private void SetViewModelSkills(SkillsViewModel vm, IList<string> baseSkills, IEnumerable<string> orderedCustomSkills)
        {
            var col1Skills = GetSkillsColumnViewModel(Column1BuiltInSkills, baseSkills);
            var col2Skills = GetSkillsColumnViewModel(Column2BuiltInSkills, baseSkills);

            var allCustomSkillViewModels = GetDraftSkillsColumnViewModel(orderedCustomSkills).ToList();
            var col1CustomSkillViewModels = allCustomSkillViewModels.Where((_, index) => index % 2 == 1);
            var col2CustomSkillViewModels = allCustomSkillViewModels.Where((_, index) => index % 2 == 0);

            vm.Column1Checkboxes = col1Skills.Union(col1CustomSkillViewModels).ToList();
            vm.Column2Checkboxes = col2Skills.Union(col2CustomSkillViewModels).ToList();
        }

    
        //
        // Custom skills have an order value pre-fixed to their name in order to retain their order.
        // This method extracts the name part and orders the returned list by the pre-fix
        //
        private static IEnumerable<string> ExtractAndSort(string[] skills)
        {
            if (skills == null || skills.Length == 0)
            {
                return skills ?? new string[0];
            }

            var skillNames = new SortedList<int, string>();

            foreach (var skillValue in skills)
            {
                var separatorIndex = skillValue.IndexOf(SortPrefixSeparator);
                var skillIndex = int.Parse(skillValue.Substring(0, separatorIndex));
                var skillName = skillValue.Substring(separatorIndex + 1);

                skillNames.Add(skillIndex, skillName);
            }

            return skillNames.Select(x => x.Value);
        }

        public async Task<OrchestratorResponse> PostSkillsEditModelAsync(SkillsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.Skills_Post);

            if (m.Skills == null)
            {
                m.Skills = new List<string>();
            }

            var baseSkills = GetBaseSkills(m.Skills).ToList();
            var customSkills = GetCustomSkills(m.Skills);
            var sortedCustomSkills = ExtractAndSort(customSkills.ToArray()).ToList();

            HandleCustomSkillChange(m, baseSkills, sortedCustomSkills);

            vacancy.Skills = baseSkills.Union(sortedCustomSkills).ToList();

            // Adding in the ordering of the custom skill entries
            m.Skills = baseSkills.Union(AddOrdering(sortedCustomSkills)).ToList(); 

            //if we are adding/removing a skill then just validate and don't persist
            var validateOnly = m.IsAddingCustomSkill || m.IsRemovingCustomSkill;

            return await ValidateAndExecute(vacancy,
                v =>
                {
                    var result = _client.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors, m, vacancy);
                    return result;
                },
                v => validateOnly ? Task.CompletedTask : _client.UpdateDraftVacancyAsync(v, user));
        }

        private IEnumerable<string> AddOrdering(List<string> extractedCustomSkills)
        {
            return extractedCustomSkills.Select((c, index) => $"{index + 1}-{c}");
        }

        protected override EntityToViewModelPropertyMappings<Vacancy, SkillsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, SkillsEditModel>
            {
                { e => e.Skills, vm => vm.Skills }
            };

            return mappings;
        }

        private IEnumerable<SkillViewModel> GetSkillsColumnViewModel(IEnumerable<string> skills, IEnumerable<string> selectedSkills)
        {
            return skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selectedSkills != null && selectedSkills.Any(s => s == c),
                Value = c
            });
        }

        private IEnumerable<SkillViewModel> GetDraftSkillsColumnViewModel(IEnumerable<string> draftSkills)
        {
            return draftSkills.Select((c, index) => new SkillViewModel
            {
                Name = c,
                Selected = true, // Always selected
                Value = $"{index + 1}-{c}"
            });
        }
        
        private IEnumerable<string> GetCustomSkills(IEnumerable<string> selected)
        {
            return selected == null ? new List<string>() : selected.Except(CandidateSkills);
        }

        private IEnumerable<string> GetBaseSkills(IEnumerable<string> selected)
        {
            return selected == null ? new List<string>() : selected.Intersect(CandidateSkills);
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

        private void HandleCustomSkillChange(SkillsEditModel m, IList<string> baseSkillList, IList<string> customSkillList)
        {
            if (m.IsAddingCustomSkill)
            {
                // If the user has tried to add a custom skill that exists in the default skills list.
                if (CandidateSkills.Contains(m.AddCustomSkillName) && !baseSkillList.Contains(m.AddCustomSkillName))
                {
                    baseSkillList.Add(m.AddCustomSkillName);
                }
                else if (!CandidateSkills.Contains(m.AddCustomSkillName) && !customSkillList.Contains(m.AddCustomSkillName))
                {
                    customSkillList.Add(m.AddCustomSkillName);
                }
            }

            if (m.IsRemovingCustomSkill)
            {
               customSkillList.Remove(m.RemoveCustomSkill);
            }
        }
    }
}