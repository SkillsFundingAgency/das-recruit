using System.Collections.Generic;
using Esfa.Recruit.Shared.Web.ViewModels;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills
{
    public class SkillsViewModel : SkillsViewModelBase
    {
        public string Title { get; internal set; }
        public string AddCustomSkillName { get; set; }
        public List<string> Skills { get; set; }
        public ReviewSummaryViewModel Review { get; set; } = new ReviewSummaryViewModel();
        public bool IsTaskListCompleted { get ; set ; }
    }
}
