using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.Models.Validators;

public class EnterLocationManuallyEditModelValidator : AbstractValidator<EnterLocationManuallyEditModel>
{
    private const int MaxLineLength = 100;
    private const int MaxPostcodeLength = 8;
    
    public EnterLocationManuallyEditModelValidator()
    {
        RuleFor(x => x.AddressLine1)
            .NotEmpty()
            .WithMessage("Enter address line 1, typically the building and street")
            .ValidFreeTextCharacters()
            .WithMessage("Address line 1 must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes")
            .MaximumLength(MaxLineLength)
            .WithMessage("Address line 1 must be {MaxLength} characters or less");
        
        RuleFor(x => x.AddressLine2)
            .ValidFreeTextCharacters()
            .WithMessage("Address line 2 must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes")
            .MaximumLength(MaxLineLength)
            .WithMessage("Address line 2 must be {MaxLength} characters or less");
        
        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("Enter town or city")
            .ValidFreeTextCharacters()
            .WithMessage("Town or city must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes")
            .MaximumLength(MaxLineLength)
            .WithMessage("Town or city must be {MaxLength} characters or less");
        
        RuleFor(x => x.County)
            .ValidFreeTextCharacters()
            .WithMessage("County must only include letters a to z, numbers 0 to 9, and special characters such as hyphens, spaces and apostrophes")
            .MaximumLength(MaxLineLength)
            .WithMessage("County must be {MaxLength} characters or less");
        
        RuleFor(x => x.Postcode)
            .NotEmpty()
            .WithMessage("Enter postcode")
            .MaximumLength(MaxPostcodeLength)
            .WithMessage("Postcode is too long. Enter a UK postcode in the format ‘SW10 1AA’")
            .ValidPostCode()
            .WithMessage("Enter a UK postcode in the format ‘SW10 1AA’ ");
    }
}