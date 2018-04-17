using System.Collections.Generic;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills
{
    public class SkillsViewModel : SkillsEditModel
    {
        public string Title { get; internal set; }
        public List<SkillViewModel> Column1Checkboxes { get; set; }
        public List<SkillViewModel> Column2Checkboxes { get; set; }
        public List<string> CustomSkills { get; set; }

        public IList<string> OrderedFieldNames => new List<string>
        {
            
        };
    }

    public class SkillViewModel
    {
        public string Name { get; set; }
        public bool Selected { get; set; }
    }
    
}
