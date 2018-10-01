using System;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class RuleTemplateRunner : IRuleTemplateRunner
    {
        private readonly IServiceProvider _provider;

        public RuleTemplateRunner(IServiceProvider provider)
        {
            _provider = provider;
        }

        public string ToText(object data, string fieldName)
        {
            if (data == null)
                return string.Empty;

            var ruleTemplateType = typeof(IRuleTemplate<>).MakeGenericType(data.GetType());
            var template = _provider.GetService(ruleTemplateType);

            if (template == null)
                throw new Exception($"Cannot resolve '{ruleTemplateType.FullName}' to a service");
            
            var renderMethod = ruleTemplateType.GetMethod(nameof(IRuleTemplate<object>.ToText));
            var result = renderMethod.Invoke(template, new []{data, fieldName});
            return (string) result;
        }
    }
}
