using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators
{
    public class UkprnValidator : PropertyValidator
    {
        public UkprnValidator() : base("'{PropertyValue}' is not a valid ukprn.") {}
        protected override bool IsValid(PropertyValidatorContext context)
        {
            var ukprn = (string)context.PropertyValue;
        
            if (string.IsNullOrWhiteSpace(ukprn)) return true;
            
            return ValidationConstants.UkprnRegex.IsMatch(ukprn);
        }
    }
}