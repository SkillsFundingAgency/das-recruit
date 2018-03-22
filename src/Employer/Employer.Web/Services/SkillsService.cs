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
            }).ToList();
        }

        public IEnumerable<SkillViewModel> GetColumn2ViewModel(IEnumerable<string> selected)
        {
            return _skillsConfig.Column2Skills.Select(c => new SkillViewModel
            {
                Name = c,
                Selected = selected != null && selected.Any(s => s == c)
            }).ToList();
        }

        public IEnumerable<string> GetCustomSkills(IEnumerable<string> selected)
        {
            return selected.Except(_skillsConfig.Column1Skills).Except(_skillsConfig.Column2Skills).ToList();
        }

    }
}
