using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.RouteModel;

namespace Esfa.Recruit.Employer.Web.ViewModels.Part2.Skills
{
    public class SkillsEditModel : VacancyRouteModel
    {
        public List<string> Skills { get; set; }

        public string RemoveCustomSkill { get; set; }

        public string AddCustomSkillAction { get; set; }

        public string AddCustomSkillName { get; set; }

        public bool IsAddingCustomSkill => !string.IsNullOrEmpty(AddCustomSkillAction);

        public bool IsRemovingCustomSkill => !string.IsNullOrWhiteSpace(RemoveCustomSkill);
    }
}
