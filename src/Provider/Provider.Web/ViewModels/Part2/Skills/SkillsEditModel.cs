using System.Collections.Generic;
using Esfa.Recruit.Provider.Web.RouteModel;
using Esfa.Recruit.Shared.Web.ViewModels.Skills;

namespace Esfa.Recruit.Provider.Web.ViewModels.Part2.Skills
{
    public class SkillsEditModel : VacancyRouteModel, ISkillsEditModel
    {
        public List<string> Skills { get; set; }

        public string RemoveCustomSkill { get; set; }

        public string AddCustomSkillAction { get; set; }

        public string AddCustomSkillName { get; set; }

        public string AddCustomSkill => !string.IsNullOrEmpty(AddCustomSkillAction) ? AddCustomSkillName : null;
    }
}
