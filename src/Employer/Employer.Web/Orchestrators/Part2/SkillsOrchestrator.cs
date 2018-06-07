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
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Entities;
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
        private IEnumerable<string> Column1Skills => CandidateSkills.Skills.Take(ColumnOneCutOffIndex);
        private IEnumerable<string> Column2Skills => CandidateSkills.Skills.Skip(ColumnOneCutOffIndex);

        public SkillsOrchestrator(IEmployerVacancyClient client, ILogger<SkillsOrchestrator> logger) : base(logger)
        {
            _client = client;
            _lazyCandidateSkills = new Lazy<CandidateSkills>(() => _client.GetCandidateSkillsAsync().Result);
        }
        
        public async Task<SkillsViewModel> GetSkillsViewModelAsync(VacancyRouteModel vrm)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, vrm, RouteNames.Skills_Get);
            
            var vm = new SkillsViewModel
            {
                Title = vacancy.Title
            };

            SetViewModelSkills(vm, vacancy.Skills);
            
            return vm;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(SkillsEditModel m)
        {
            var vm = await GetSkillsViewModelAsync((VacancyRouteModel)m);

            SetViewModelSkills(vm, m.Skills);

            vm.AddCustomSkillName = m.AddCustomSkillName;
            
            return vm;
        }

        public void SetViewModelSkills(SkillsViewModel vm, IList<string> selectedSkills)
        {
            vm.Column1Checkboxes = GetSkillsColumnViewModel(Column1Skills, selectedSkills).ToList();
            vm.Column2Checkboxes = GetSkillsColumnViewModel(Column2Skills, selectedSkills).ToList();
            vm.CustomSkills = GetCustomSkills(selectedSkills).ToList();
        }

        public async Task<OrchestratorResponse> PostSkillsEditModelAsync(SkillsEditModel m, VacancyUser user)
        {
            var vacancy = await Utility.GetAuthorisedVacancyForEditAsync(_client, m, RouteNames.Skills_Post);

            if (m.Skills == null)
            {
                m.Skills = new List<string>();
            }

            HandleCustomSkillChange(m);
            
            vacancy.Skills = SortSkills(m.Skills).ToList();
            m.Skills = vacancy.Skills;

            //if we are adding/removing a skill then just validate and don't persist
            var validateOnly = m.IsAddingCustomSkill || m.IsRemovingCustomSkill;

            return await ValidateAndExecute(vacancy,
                v =>
                {
                    var result = _client.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors, m);
                    return result;
                },
                v => validateOnly ? Task.CompletedTask : _client.UpdateVacancyAsync(v, user));
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
                Selected = selectedSkills != null && selectedSkills.Any(s => s == c)
            });
        }
        
        private IEnumerable<string> GetCustomSkills(IEnumerable<string> selected)
        {
            if (selected == null)
            {
                return new List<string>();
            }

            return selected.Except(CandidateSkills.Skills);
        }

        private IEnumerable<string> SortSkills(IList<string> selected)
        {
            var filteredSelectedSkills = selected.Distinct();

            var orderedSkills = CandidateSkills.Skills
                .Union(selected);

            return orderedSkills.Where(filteredSelectedSkills.Contains);
        }

        private void SyncErrorsAndModel(IList<EntityValidationError> errors, SkillsEditModel m)
        {
            var skillsPropertyName = nameof(Vacancy.Skills);

            //Get the first invalid skill
            var skillError = errors.FirstOrDefault(e => e.PropertyName.StartsWith($"{skillsPropertyName}["));
            if (skillError == null)
            {
                return;
            }

            //Populate AddCustomSkillName so we can edit the invalid skill
            var invalidSkill = m.Skills[skillError.GetIndexPosition().Value];
            m.AddCustomSkillName = invalidSkill;
            m.Skills.Remove(invalidSkill);

            //Attach the error to AddCustomSkillName
            skillError.PropertyName = nameof(m.AddCustomSkillName);

            //Remove other skill errors
            errors.Where(e => e.PropertyName.StartsWith($"{skillsPropertyName}[")).ToList()
                .ForEach(r => errors.Remove(r));
        }

        private void HandleCustomSkillChange(SkillsEditModel m)
        {
            if (m.IsAddingCustomSkill)
            {
                m.Skills.Add(m.AddCustomSkillName);
            }

            if (m.IsRemovingCustomSkill)
            {
                m.Skills.Remove(m.RemoveCustomSkill);
            }
        }
    }
}