using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills;

namespace Esfa.Recruit.Employer.Web.Services
{
    public interface ISkillsService
    {
        IEnumerable<SkillViewModel> GetColumn1ViewModel(IEnumerable<string> selected);
        IEnumerable<SkillViewModel> GetColumn2ViewModel(IEnumerable<string> selected);
        IEnumerable<string> GetCustomSkills(IEnumerable<string> selected);
        IEnumerable<string> SortSkills(IEnumerable<string> selected);
    }
}
