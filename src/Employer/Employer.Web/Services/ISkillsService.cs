using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface ISkillsService
    {
        List<SkillViewModel> GetColumn1ViewModel(List<string> selected);
        List<SkillViewModel> GetColumn2ViewModel(List<string> selected);
        List<string> GetCustomSkills(List<string> selected);
        List<string> SortSkills(List<string> selected);
    }
}
