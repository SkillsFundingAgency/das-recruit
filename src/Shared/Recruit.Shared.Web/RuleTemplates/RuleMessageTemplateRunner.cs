using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Application.Rules;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class RuleTemplateMessageRunner : IRuleMessageTemplateRunner
    {
        public string ToText(RuleId ruleId, object data, string fieldName)
        {
            switch (ruleId)
            {
                case RuleId.ProfanityChecks:
                    return ProfanityRuleMessageTemplate.ToText(data as ProfanityData, fieldName);
                case RuleId.BannedPhraseChecks:
                    return BannedPhraseMessageTemplate.ToText(data as BannedPhrasesData, fieldName);
                case RuleId.TitlePopularity:
                    return VacancyTitlePopularityRuleMessageTemplate.ToText(data as TitlePopularityData, fieldName);
                default:
                    throw new Exception($"Cannot resolve ruleId: {ruleId} to a rule message template formatter.");
            }
        }
    }
}
