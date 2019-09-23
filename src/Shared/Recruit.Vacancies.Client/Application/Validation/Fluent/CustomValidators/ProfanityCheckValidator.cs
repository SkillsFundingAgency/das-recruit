using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    public class ProfanityCheckValidator : PropertyValidator
    {
        private readonly IProfanityListProvider _profanityListProvider;

        public ProfanityCheckValidator(IProfanityListProvider profanityListProvider) 
            : base("'{PropertyValue}' must not contain a banned word or phrase.")
        {
            _profanityListProvider = profanityListProvider;
        }
        protected override bool IsValid(PropertyValidatorContext context)
        {
            var profanityList = _profanityListProvider.GetProfanityListAsync();

            var freeText = (string)context.PropertyValue;

            if (profanityList.Result.Any(freeText.Contains))
                return false;
            return true;
        }
    }
}