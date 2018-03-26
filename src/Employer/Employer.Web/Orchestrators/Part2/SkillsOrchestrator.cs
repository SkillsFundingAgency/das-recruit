using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;

namespace Esfa.Recruit.Employer.Web.Orchestrators.Part2
{
    public class SkillsOrchestrator
    {
        private readonly IVacancyClient _client;
        private readonly ISkillsService _skillsService;
        
        public SkillsOrchestrator(IVacancyClient client, ISkillsService skillsService)
        {
            _client = client;
            _skillsService = skillsService;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(Guid vacancyId)
        {
            var vacancy = await _client.GetVacancyForEditAsync(vacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }

            var vm = new SkillsViewModel
            {
                Column1Checkboxes = _skillsService.GetColumn1ViewModel(vacancy.Skills),
                Column2Checkboxes = _skillsService.GetColumn2ViewModel(vacancy.Skills),
                CustomSkills = _skillsService.GetCustomSkills(vacancy.Skills)
            };

            return vm;
        }

        public async Task<SkillsViewModel> GetSkillsViewModelAsync(SkillsEditModel m)
        {
            var vm = await GetSkillsViewModelAsync(m.VacancyId);

            vm.Column1Checkboxes = _skillsService.GetColumn1ViewModel(m.Skills);
            vm.Column2Checkboxes = _skillsService.GetColumn2ViewModel(m.Skills);
            vm.CustomSkills = _skillsService.GetCustomSkills(m.Skills);
            vm.AddCustomSkillName = m.AddCustomSkillName;

            return vm;
        }

        public async Task PostSkillsEditModelAsync(SkillsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }
            
            var skills = m.Skills?.ToList() ?? new List<string>();
            
            if (!string.IsNullOrEmpty(m.AddCustomSkillAction) && !string.IsNullOrWhiteSpace(m.AddCustomSkillName))
            { 
                skills.Add(m.AddCustomSkillName);
            }

            if (!string.IsNullOrWhiteSpace(m.RemoveCustomSkill))
            {
                skills.Remove(m.RemoveCustomSkill);
            }

            vacancy.Skills = _skillsService.SortSkills(skills);

            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}

