using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class ProfanityRuleMessageTemplate : IRuleMessageTemplate
    {
        public string ToText(object data, string fieldName)
        {
            return (data is ProfanityData profanityData) ? 
                $"{fieldName} contains the phrase {profanityData.Profanity}" : 
                string.Empty;
        }
    }
}
