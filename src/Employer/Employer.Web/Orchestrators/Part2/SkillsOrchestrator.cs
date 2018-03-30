using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class SkillsOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly SkillsConfiguration _skillsConfig;

        public SkillsOrchestrator(IVacancyClient client, IOptions<SkillsConfiguration> skillsConfigOptions)
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

        public async Task PostSkillsEditModelAsync(SkillsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }
            
            var skills = m.Skills ?? new List<string>();
            
            vacancy.Skills = SortSkills(skills).ToList();

            await _client.UpdateVacancyAsync(vacancy);
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
    }
}

