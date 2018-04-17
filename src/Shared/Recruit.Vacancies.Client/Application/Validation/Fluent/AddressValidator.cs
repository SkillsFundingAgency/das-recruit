using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    internal class AddressValidator : AbstractValidator<Address> 
    {
        internal AddressValidator(long ruleId)
        {
            RuleFor(x => x.AddressLine1)
                .NotEmpty()
                    .WithMessage("You must enter an address")
                    .WithErrorCode("5")
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(100)
                    .WithMessage("The address must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .WithRuleId(ruleId);

            RuleFor(x => x.AddressLine2)
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(100)
                    .WithMessage("The address must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .WithRuleId(ruleId);
            
            RuleFor(x => x.AddressLine3)
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(100)
                    .WithMessage("The address must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .WithRuleId(ruleId);
            
            RuleFor(x => x.AddressLine4)
                .ValidFreeTextCharacters()
                    .WithMessage("You have entered invalid characters")
                    .WithErrorCode("6")
                .MaximumLength(100)
                    .WithMessage("The address must not be more than {MaxLength} characters")
                    .WithErrorCode("7")
                .WithRuleId(ruleId);

            RuleFor(x => x.Postcode)
                .NotEmpty()
                    .WithMessage("Enter the postcode")
                    .WithErrorCode("8")
                .PostCode()
                    .When(x => !string.IsNullOrEmpty(x.Postcode), ApplyConditionTo.CurrentValidator)
                    .WithMessage("'{PropertyName}' is not a valid format")
                    .WithErrorCode("9")
                .WithRuleId(ruleId);
		}
	}
}
