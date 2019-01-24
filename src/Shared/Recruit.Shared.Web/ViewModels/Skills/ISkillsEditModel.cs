using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels.Skills
{
    public interface ISkillsEditModel
    {
        List<string> Skills { get; set; }
        string RemoveCustomSkill { get; }
        string AddCustomSkill { get; }
    }
}