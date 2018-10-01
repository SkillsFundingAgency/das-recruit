namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public interface IRuleTemplate<in T>
    {
        string ToText(T data, string fieldName);
    }
}
