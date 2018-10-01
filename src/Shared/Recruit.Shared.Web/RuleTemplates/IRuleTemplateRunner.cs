using System;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public interface IRuleTemplateRunner
    {
        string ToText(object data, string fieldName);
    }
}
