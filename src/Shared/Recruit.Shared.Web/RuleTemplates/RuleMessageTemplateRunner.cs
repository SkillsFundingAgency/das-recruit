using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Rules;
using Newtonsoft.Json;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class RuleMessageTemplateRunner : IRuleMessageTemplateRunner
    {
        public string ToText(RuleId ruleId, string data, string fieldName)
        {
            switch (ruleId)
            {
                case RuleId.ProfanityChecks:
                    return ProfanityRuleMessageTemplate.ToText(JsonConvert.DeserializeObject<ProfanityData>(data), fieldName);
                case RuleId.BannedPhraseChecks:
                    return BannedPhraseMessageTemplate.ToText(JsonConvert.DeserializeObject<BannedPhrasesData>(data), fieldName);
                case RuleId.VacancyAnonymous:
                    return VacancyAnonymousMessageTemplate.ToText();
                default:
                    throw new Exception($"Cannot resolve ruleId: {ruleId} to a rule message template formatter.");
            }
        }
    }
}
