using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills
{
    public class SkillsEditModel : VacancyRouteModel, ISkillsEditModel
    {
        public List<string> Skills { get; set; }

        public string AddCustomSkillAction { get; set; }

        public string AddCustomSkillName { get; set; }

        public string AddCustomSkill => !string.IsNullOrEmpty(AddCustomSkillAction) ? AddCustomSkillName : null;
    }
}
