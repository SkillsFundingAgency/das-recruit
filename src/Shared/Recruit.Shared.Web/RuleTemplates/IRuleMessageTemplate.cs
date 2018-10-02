namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public interface IRuleMessageTemplate
    {
        string ToText(object data, string fieldName);
    }
}
