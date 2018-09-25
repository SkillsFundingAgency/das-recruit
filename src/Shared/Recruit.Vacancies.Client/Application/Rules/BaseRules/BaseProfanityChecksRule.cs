using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Esfa.QA.Core.Extensions;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Engine;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Application.Rules.BaseRules
{
    public abstract class BaseProfanityChecksRule : Rule
    {
        public enum ConsolidationOption
        {
            NoConsolidation,
            ConsolidateByField
        }

        private readonly IProfanityListProvider _profanityListProvider;
        private readonly ConsolidationOption _consolidationOption;

        protected BaseProfanityChecksRule(string ruleId, IProfanityListProvider profanityListProvider, ConsolidationOption consolidationOption, decimal weighting = 1.0m) : base(ruleId, weighting)
        {
            _profanityListProvider = profanityListProvider;
            _consolidationOption = consolidationOption;
        }

        protected async Task<IEnumerable<RuleOutcome>> ProfanityCheckAsync(Expression<Func<string>> property, string relatedFieldId = null)
        {
            var fieldId = relatedFieldId ?? property.GetQualifiedFieldId();

            var foundProfanities = await FindOccurrencesAsync(property);

            if (foundProfanities.Values.Sum() > 0)
            {
                switch (_consolidationOption)
                {
                    case ConsolidationOption.NoConsolidation:
                        return CreateUnconsolidatedOutcomes(foundProfanities, fieldId);

                    case ConsolidationOption.ConsolidateByField:
                        return CreateConsolidatedOutcomes(foundProfanities, fieldId);

                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return new[] {CreateOutcome(0, $"No profanities found in '{fieldId}'", fieldId)};
        }

        private async Task<Dictionary<string, int>> FindOccurrencesAsync(Expression<Func<string>> property)
        {
            var foundProfanities = new Dictionary<string, int>();
            var value = property.Compile()();
            var checkValue = value.FormatForParsing();
            if (string.IsNullOrWhiteSpace(checkValue)) return foundProfanities;

            var profanities = await _profanityListProvider.GetProfanityListAsync();

            foreach (var profanity in profanities)
            {
                var occurrences = checkValue.CountOccurrences(profanity);

                if (occurrences > 0)
                {
                    if (!foundProfanities.ContainsKey(profanity)) foundProfanities.Add(profanity, 0);

                    foundProfanities[profanity] += occurrences;
                }
            }

            return foundProfanities;
        }

        private IEnumerable<RuleOutcome> CreateUnconsolidatedOutcomes(Dictionary<string, int> foundProfanities, string fieldId)
        {
            return foundProfanities
                .Select(foundProfanity =>
                {
                    var count = foundProfanity.Value;
                    var term = foundProfanity.Key;
                    var foundMessage = count > 1 ? $"found {count} times" : "found";

                    return CreateOutcome(count, $"Profanity '{term}' {foundMessage} in '{fieldId}'", fieldId);
                });
        }

        private IEnumerable<RuleOutcome> CreateConsolidatedOutcomes(Dictionary<string, int> foundProfanities, string fieldId)
        {
            var count = foundProfanities.Values.Sum();
            var terms = string.Join(",", foundProfanities.Keys);

            return new[]
            {
                CreateOutcome(count, $"{count} profanities '{terms}' found in '{fieldId}'", fieldId)
            };
        }
    }
}
