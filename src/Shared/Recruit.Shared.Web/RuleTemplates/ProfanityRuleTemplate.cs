using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class ProfanityRuleTemplate : IRuleTemplate<ProfanityData>
    {
        public string ToText(ProfanityData data, string fieldName)
        {
            return $"{fieldName} contains the phrase {data.Profanity}";
        }
    }
}
