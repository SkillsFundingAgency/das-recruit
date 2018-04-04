using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.Extensions;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class SkillsOrchestrator : EntityValidatingOrchestrator<Vacancy, SkillsEditModel>
    {
        private const VacancyRuleSet ValidationRules = VacancyRuleSet.Skills;
        private readonly IVacancyClient _client;
        private readonly SkillsConfiguration _skillsConfig;

        public SkillsOrchestrator(IVacancyClient client, IOptions<SkillsConfiguration> skillsConfigOptions, ILogger<SkillsOrchestrator> logger) : base(logger)
        {
            _client = client;
            _skillsConfig = skillsConfigOptions.Value;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new SkillsViewModel();

            SetViewModelSkills(vm, vacancy.Skills);
            
            return vm;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(SkillsEditModel m)
        {
            var vm = await GetSkillsViewModelAsync(m.VacancyId);

            SetViewModelSkills(vm, m.Skills);

            vm.AddCustomSkillName = m.AddCustomSkillName;
            
            return vm;
        }

        public void SetViewModelSkills(SkillsViewModel vm, IEnumerable<string> skills)
        {
            vm.Column1Checkboxes = GetColumn1ViewModel(skills).ToList();
            vm.Column2Checkboxes = GetColumn2ViewModel(skills).ToList();
            vm.CustomSkills = GetCustomSkills(skills).ToList();
        }

        public async Task<OrchestratorResponse> PostSkillsEditModelAsync(SkillsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }
            
            var skills = m.Skills ?? new List<string>();
            
            vacancy.Skills = SortSkills(skills).ToList();

            return await ValidateAndExecute(vacancy,
                v =>
                {
                    var result = _client.Validate(v, ValidationRules);
                    SyncErrorsAndModel(result.Errors, m);
                    return result;
                },
                v => _client.UpdateVacancyAsync(vacancy, false)
            );
        }
        
        protected override EntityToViewModelPropertyMappings<Vacancy, SkillsEditModel> DefineMappings()
        {
            var mappings = new EntityToViewModelPropertyMappings<Vacancy, SkillsEditModel>();

            mappings.Add(e => e.Skills, vm => vm.Skills);

            return mappings;
        }

        private IEnumerable<SkillViewModel> GetColumn1ViewModel(IEnumerable<string> selected)
        {
            return _skillsConfig.Column1Skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selected != null && selected.Any(s => s == c)
            });
        }

        private IEnumerable<SkillViewModel> GetColumn2ViewModel(IEnumerable<string> selected)
        {
            return _skillsConfig.Column2Skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selected != null && selected.Any(s => s == c)
            });
        }

        private IEnumerable<string> GetCustomSkills(IEnumerable<string> selected)
        {
            if (selected == null)
            {
                return new List<string>();
            }

            return selected.Except(_skillsConfig.Column1Skills).Except(_skillsConfig.Column2Skills);
        }

        private IEnumerable<string> SortSkills(IEnumerable<string> selected)
        {
            var filteredSelectedSkills = selected.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

            var orderedSkills = _skillsConfig.Column1Skills
                .Union(_skillsConfig.Column2Skills)
                .Union(selected)
                .ToList();

            return orderedSkills.Where(filteredSelectedSkills.Contains);
        }

        private void SyncErrorsAndModel(IList<EntityValidationError> errors, SkillsEditModel m)
        {
            //Get the first invalid skill
            var skillError = errors.FirstOrDefault(e => e.PropertyName.StartsWith($"{nameof(m.Skills)}["));
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
            errors.Where(e => e.PropertyName.StartsWith($"{nameof(m.Skills)}[")).ToList()
                .ForEach(r => errors.Remove(r));
        }
    }
}

