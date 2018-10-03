using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public class BannedPhraseMessageTemplate : IRuleMessageTemplate
    {
        public string ToText(object data, string fieldName)
        {
            return (data is BannedPhrasesData bannedPhrasesData) ?
                $"{fieldName} contains the phrase {bannedPhrasesData.BannedPhrase}" :
                string.Empty;
        }
    }
}
