using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Shared.Web.RuleTemplates
{
    public static class BannedPhraseMessageTemplate
    {
        public static string ToText(BannedPhrasesData msgData, string fieldName)
        {
            var baseMessage = $"{fieldName} contains the phrase '{msgData.BannedPhrase}'";
            return msgData.Occurrences > 1 
                ? $"{baseMessage} {msgData.Occurrences} times"
                : $"{baseMessage}";
        }
    }
}