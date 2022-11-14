using System.Collections.Generic;

namespace Esfa.Recruit.Shared.Web.ViewModels.Skills
{
    public interface ISkillsViewModel
    {
        List<SkillViewModel> Column1Checkboxes { get; set; }
        List<SkillViewModel> Column2Checkboxes { get; set; }
    }
}