using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Services;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Esfa.Recruit.Vacancies.Client.Domain;
using Esfa.Recruit.Vacancies.Client.Domain.Exceptions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Client;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Internal;

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
            vm.Column1Checkboxes = _skillsService.GetColumn1ViewModel(skills).ToList();
            vm.Column2Checkboxes = _skillsService.GetColumn2ViewModel(skills).ToList();
            vm.CustomSkills = _skillsService.GetCustomSkills(skills).ToList();
        }

        public async Task PostSkillsEditModelAsync(SkillsEditModel m)
        {
            var vacancy = await _client.GetVacancyForEditAsync(m.VacancyId);

            if (!vacancy.CanEdit)
            {
                throw new ConcurrencyException(string.Format(ErrorMessages.VacancyNotAvailableForEditing, vacancy.Title));
            }
            
            var skills = m.Skills ?? new List<string>();
            
            vacancy.Skills = _skillsService.SortSkills(skills).ToList();

            await _client.UpdateVacancyAsync(vacancy, false);
        }
    }
}

