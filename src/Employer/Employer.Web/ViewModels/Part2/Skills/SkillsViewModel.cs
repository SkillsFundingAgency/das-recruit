using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills
{
    public class SkillsViewModel : SkillsEditModel
    {
        public IEnumerable<SkillViewModel> Column1Checkboxes { get; set; }
        public IEnumerable<SkillViewModel> Column2Checkboxes { get; set; }
        public IEnumerable<string> CustomSkills { get; set; }
    }

    public class SkillViewModel
    {
        public string Name { get; set; }
        public bool Selected { get; set; }

        public string Id => Name.Replace(" ", "-").ToLower();
    }
    
}
