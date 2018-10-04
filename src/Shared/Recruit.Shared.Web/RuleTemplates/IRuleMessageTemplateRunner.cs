﻿using Esfa.Recruit.Vacancies.Client.Application.Rules;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public interface IRuleMessageTemplateRunner
    {
        string ToText(RuleId ruleId, string data, string fieldName);
    }
}
