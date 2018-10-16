using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public static class ProfanityRuleMessageTemplate
    {
        public static string ToText(ProfanityData msgData, string fieldName)
        {
            return msgData.Occurrences > 1 
                ? $"{fieldName} contains the phrase {msgData.Profanity} {msgData.Occurrences} times" 
                : $"{fieldName} contains the phrase {msgData.Profanity}";
        }
    }
}
