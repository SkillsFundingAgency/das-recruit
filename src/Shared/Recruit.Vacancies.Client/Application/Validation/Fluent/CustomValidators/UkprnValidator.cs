using FluentValidation;
using FluentValidation.Validators;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent.CustomValidators;

public class UkprnValidator<T, TProperty> : PropertyValidator<T, TProperty>
{
    public override string Name => "UkprnValidator";
    protected override string GetDefaultMessageTemplate(string errorCode)
    {
        return base.GetDefaultMessageTemplate("'{PropertyValue}' is not a valid ukprn.");
    }

    public override bool IsValid(ValidationContext<T> context, TProperty PropertyValue)
    {
        var ukprn = PropertyValue as string;
        
        if (string.IsNullOrWhiteSpace(ukprn)) return true;
            
        return ValidationConstants.UkprnRegex.IsMatch(ukprn);
    }
}