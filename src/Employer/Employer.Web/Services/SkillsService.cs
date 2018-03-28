using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.Configuration;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Employer.Web.Services
{
    public class SkillsService : ISkillsService
    {

        private readonly SkillsConfiguration _skillsConfig;

        public SkillsService(IOptions<SkillsConfiguration> skillsConfigOptions)
        {
            _skillsConfig = skillsConfigOptions.Value;
        }

        public IEnumerable<SkillViewModel> GetColumn1ViewModel(IEnumerable<string> selected)
        {
            return _skillsConfig.Column1Skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selected != null && selected.Any(s => s == c)
            });
        }

        public IEnumerable<SkillViewModel> GetColumn2ViewModel(IEnumerable<string> selected)
        {
            return _skillsConfig.Column2Skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selected != null && selected.Any(s => s == c)
            });
        }

        public IEnumerable<string> GetCustomSkills(IEnumerable<string> selected)
        {
            if (selected == null)
            {
                return new List<string>();
            }

            return selected.Except(_skillsConfig.Column1Skills).Except(_skillsConfig.Column2Skills);
        }

        public IEnumerable<string> SortSkills(IEnumerable<string> selected)
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
