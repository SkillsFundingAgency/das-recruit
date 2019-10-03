using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    internal class ContactDetailValidator : AbstractValidator<ContactDetail> 
    {
        internal ContactDetailValidator(long ruleId, IProfanityListProvider profanityListProvider)
        {
            RuleFor(x => x.Name)
                .MaximumLength(100)
                    .WithMessage("Contact name must not exceed {MaxLength} characters")
                    .WithErrorCode("90")
                .ValidFreeTextCharacters()
                    .WithMessage("Contact name contains some invalid characters")
                    .WithErrorCode("91")
                .ProfanityCheck(profanityListProvider)
                .WithMessage("Contact name must not contain a banned word or phrase.")
                .WithErrorCode("615")
                .WithRuleId(ruleId);

            RuleFor(x => x.Email)
                .MaximumLength(100)
                    .WithMessage("Email address must not exceed {MaxLength} characters")
                    .WithErrorCode("92")
                .ValidFreeTextCharacters()
                    .WithMessage("Email address contains some invalid characters")
                    .WithErrorCode("93")
                .Matches(ValidationConstants.EmailAddressRegex)
                    .WithMessage("Email address must be in a valid format")
                    .WithErrorCode("94")
                    .When(v => !string.IsNullOrEmpty(v.Email))
                .ProfanityCheck(profanityListProvider)
                .WithMessage("Email address must not contain a banned word or phrase.")
                .WithErrorCode("616")
                .WithRuleId(ruleId);

            RuleFor(x => x.Phone)
                .MaximumLength(16)
                    .WithMessage("Contact number must not exceed {MaxLength} digits")
                    .WithErrorCode("95")
                .MinimumLength(8)
                    .WithMessage("Contact number must be more than {MinLength} digits")
                    .WithErrorCode("96")
                .Matches(ValidationConstants.PhoneNumberRegex)
                    .WithMessage("Contact number contains some invalid characters")
                    .WithErrorCode("97")
                    .When(v => !string.IsNullOrEmpty(v.Phone))
                .WithRuleId(ruleId);
        }
    }
}
