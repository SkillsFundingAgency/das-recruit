using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules
{
    public class BaseBannedPhraseChecksRule : Rule
    {
        private readonly ConsolidationOption _consolidationOption;

        protected IEnumerable<string> BannedPhrases { get; set; } = new List<string>();

        public BaseBannedPhraseChecksRule(
            string ruleId, ConsolidationOption consolidationOption, decimal weighting = 1.0m) 
            : base(ruleId, weighting)
        {
            _consolidationOption = consolidationOption;
        }

        protected async Task<IEnumerable<RuleOutcome>> BannedPhraseCheckAsync(Expression<Func<string>> property, string relatedFieldId = null)
        {
            var fieldId = relatedFieldId ?? property.GetQualifiedFieldId();

            var foundBannedPhrases = await FindOccurrencesAsync(property);

            if (foundBannedPhrases.Values.Sum() > 0)
                switch (_consolidationOption)
                {
                    case ConsolidationOption.NoConsolidation:
                        return CreateUnconsolidatedOutcomes(foundBannedPhrases, fieldId);

                    case ConsolidationOption.ConsolidateByField:
                        return CreateConsolidatedOutcomes(foundBannedPhrases, fieldId);

                    default:
                        throw new ArgumentOutOfRangeException();
                }

            return new[] { CreateOutcome(0, $"No banned phrases found in '{fieldId}'", null, fieldId) };
        }

        private async Task<Dictionary<string, int>> FindOccurrencesAsync(Expression<Func<string>> property)
        {
            var value = property.Compile()();
            var checkValue = value.FormatForParsing();

            var foundBannedPhrases = new Dictionary<string, int>();

            foreach (var bannedPhrase in BannedPhrases)
            {
                var occurrences = checkValue.CountOccurrences(bannedPhrase);

                if (occurrences > 0)
                {
                    if (!foundBannedPhrases.ContainsKey(bannedPhrase)) foundBannedPhrases.Add(bannedPhrase, 0);

                    foundBannedPhrases[bannedPhrase] += occurrences;
                }
            }

            return foundBannedPhrases;
        }

        private IEnumerable<RuleOutcome> CreateUnconsolidatedOutcomes(Dictionary<string, int> foundBannedPhrases, string fieldId)
        {
            return foundBannedPhrases
                .Select(foundBannedPhrase =>
                {
                    var count = foundBannedPhrase.Value;
                    var term = foundBannedPhrase.Key;
                    var foundMessage = count > 1 ? $"found {count} times" : "found";
                    var narrative = $"Banned phrase '{term}' {foundMessage} in '{fieldId}'";

                    var data = new BannedPhrasesData {BannedPhrase = term};
                    return CreateOutcome(count, narrative, data, fieldId);
                });
        }

        private IEnumerable<RuleOutcome> CreateConsolidatedOutcomes(Dictionary<string, int> foundBannedPhrases, string fieldId)
        {
            var count = foundBannedPhrases.Values.Sum();
            var terms = string.Join(",", foundBannedPhrases.Keys);
            var narrative = $"{count} profanities '{terms}' found in '{fieldId}'";
            var data = new BannedPhrasesData { BannedPhrase = terms };

            return new[]
            {
                CreateOutcome(count, narrative, data, fieldId)
            };            
        }
    }
}
