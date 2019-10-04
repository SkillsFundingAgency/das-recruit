using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Rules.Extensions;
using Esfa.Recruit.Vacancies.Client.Infrastructure.ReferenceData.Profanities;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    public class ProfanityCheckValidator : PropertyValidator
    {
        private readonly IProfanityListProvider _profanityListProvider;

        public ProfanityCheckValidator(IProfanityListProvider profanityListProvider) 
            : base("{PropertyName} must not contain a banned word or phrase.")
        {
            _profanityListProvider = profanityListProvider;
        }
        protected override bool IsValid(PropertyValidatorContext context)
        {
            var profanityList = _profanityListProvider.GetProfanityListAsync();

            var freeText = (string) context.PropertyValue;

            var formatForParsing = freeText.FormatForParsing();

            foreach (var profanity in profanityList.Result)
            {
                if (freeText != null)
                {
                    var occurrences = formatForParsing.CountOccurrences(profanity);

                    if (occurrences > 0)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}