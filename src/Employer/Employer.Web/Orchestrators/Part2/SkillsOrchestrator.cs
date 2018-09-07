using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration.Routing;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Skills;
using Microsoft.Extensions.Logging;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class SkillsOrchestrator : EntityValidatingOrchestrator<Vacancy, SkillsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Skills;
        private const int ColumnOneCutOffIndex = 9;

        private readonly IEmployerVacancyClient _client;
        private readonly Lazy<CandidateSkills> _lazyCandidateSkills;

        private CandidateSkills CandidateSkills => _lazyCandidateSkills.Value;
        private IEnumerable<string> Column1BuiltInSkills => CandidateSkills.Skills.Take(ColumnOneCutOffIndex);
        private IEnumerable<string> Column2BuiltInSkills => CandidateSkills.Skills.Skip(ColumnOneCutOffIndex);

        public SkillsOrchestrator(IEmployerVacancyClient client, ILogger<SkillsOrchestrator> logger) : base(logger)
        {
            _client = client;
            _lazyCandidateSkills = new Lazy<CandidateSkills>(() => _client.GetCandidateSkillsAsync().Result);
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
                SetViewModelSkills(vm, vacancy.Skills);
            }
            else
            {
                SetViewModelSkills(vm, draftSkills.ToList(), true);
            }
            
            return vm;
        }

        private static IEnumerable<string> ExtractAndSort(string[] skills)
        {
            if (skills == null || skills.Length == 0)
            {
                return skills ?? new string[0];
            }

            var skillNames = new SortedList<int, string>();

            foreach (var skillValue in skills)
            {
                var separatorIndex = skillValue.IndexOf('-');
                if (int.TryParse(skillValue.Substring(0, separatorIndex), out int skillIndex))
                {
                    var skillName = skillValue.Substring(separatorIndex + 1);
                    skillNames.Add(skillIndex, skillName);
                }
                else
                {
                    skillNames.Add(skillIndex, skillValue);
                }
            }

            return skillNames.Select(x => x.Value);
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(SkillsEditModel m)
        {
            var vm = await GetSkillsViewModelAsync((VacancyRouteModel)m);

            SetViewModelSkills(vm, m.Skills, true);

            vm.AddCustomSkillName = m.AddCustomSkillName;
            
            return vm;
        }

        public void SetViewModelSkills(SkillsViewModel vm, IList<string> skills = null, bool areDraftSkills = false)
        {
            IEnumerable<string> orderedCustomSkills;
            if (areDraftSkills)
            {
                orderedCustomSkills = ExtractAndSort(GetCustomSkills(skills).ToArray());
            }
            else
            {
                orderedCustomSkills = GetCustomSkills(skills).ToArray();
            }

            var baseSkills = GetBaseSkills(skills).ToArray();

            var col1Skills = GetSkillsColumnViewModel(Column1BuiltInSkills, baseSkills);
            var col2Skills = GetSkillsColumnViewModel(Column2BuiltInSkills, baseSkills);

            var allDraftSkills = GetDraftSkillsColumnViewModel(orderedCustomSkills);
            var col1DraftSkills = allDraftSkills.Where((_, index) => index % 2 == 1);
            var col2DraftSkills = allDraftSkills.Where((_, index) => index % 2 == 0);

            vm.Column1Checkboxes = col1Skills.Union(col1DraftSkills).ToList();
            vm.Column2Checkboxes = col2Skills.Union(col2DraftSkills).ToList();
        }

        public async Task<OrchestratorResponse> PostSkillsEditModelAsync(SkillsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.Skills_Post);

            if (m.Skills == null)
            {
                m.Skills = new List<string>();
            }

            var baseSkills = GetBaseSkills(m.Skills);
            var customSkills = GetCustomSkills(m.Skills);
            var extractedCustomSkills = ExtractAndSort(customSkills.ToArray()).ToList();

            HandleCustomSkillChange(m, extractedCustomSkills);

            extractedCustomSkills = extractedCustomSkills.Distinct().ToList();
            
            vacancy.Skills = baseSkills.Union(extractedCustomSkills).ToList();

            m.Skills = baseSkills.Union(AddOrdering(extractedCustomSkills)).ToList();

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
            return selected == null ? new List<string>() : selected.Except(CandidateSkills.Skills);
        }

        private IEnumerable<string> GetBaseSkills(IEnumerable<string> selected)
        {
            return selected == null ? new List<string>() : selected.Intersect(CandidateSkills.Skills);
        }

        private IEnumerable<string> SortSkills(IList<string> selected)
        {
            var filteredSelectedSkills = selected.Distinct();

            var orderedSkills = CandidateSkills.Skills
                .Union(selected);

            return orderedSkills.Where(filteredSelectedSkills.Contains);
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
            var invalidSkill = vacancy.Skills[skillError.GetIndexPosition().Value];
            m.AddCustomSkillName = invalidSkill;
            m.Skills.RemoveAt(skillError.GetIndexPosition().Value);
            vacancy.Skills.RemoveAt(skillError.GetIndexPosition().Value);

            //Attach the error to AddCustomSkillName
            skillError.PropertyName = nameof(m.AddCustomSkillName);

            //Remove other skill errors
            errors.Where(e => e.PropertyName.StartsWith($"{skillsPropertyName}[")).ToList()
                .ForEach(r => errors.Remove(r));
        }

        private void HandleCustomSkillChange(SkillsEditModel m, IList<string> customSkillList)
        {
            if (m.IsAddingCustomSkill)
            {
                customSkillList.Add(m.AddCustomSkillName);
            }

            if (m.IsRemovingCustomSkill)
            {
               customSkillList.Remove(m.RemoveCustomSkill);
            }
        }
    }
}