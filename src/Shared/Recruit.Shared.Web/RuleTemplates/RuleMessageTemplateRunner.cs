using System;
using Esfa.Recruit.Vacancies.Client.Application.Rules;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class RuleTemplateMessageRunner : IRuleMessageTemplateRunner
    {
        public string ToText(RuleId ruleId, object data, string fieldName)
        {
            IRuleMessageTemplate template;
            switch (ruleId)
            {
                case RuleId.ProfanityChecks:
                    template = new ProfanityRuleMessageTemplate();
                    break;
                case RuleId.BannedPhraseChecks:
                    template = new BannedPhraseMessageTemplate();
                    break;
                default:
                    throw new Exception($"Cannot resolve ruleId:{ruleId} to a rule message template");
            }

            return template.ToText(data, fieldName);
        }
    }
}
