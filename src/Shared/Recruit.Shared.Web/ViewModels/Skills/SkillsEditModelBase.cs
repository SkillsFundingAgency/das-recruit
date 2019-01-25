using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels.Skills
{
    public class SkillsEditModelBase
    {
        public List<string> Skills { get; set; }
        public string AddCustomSkillAction { get; set; }
        public string AddCustomSkillName { get; set; }

        public bool IsAddingCustomSkill => !string.IsNullOrEmpty(AddCustomSkillAction);
    }
}