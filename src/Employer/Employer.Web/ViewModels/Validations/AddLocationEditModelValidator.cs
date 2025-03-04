using Esfa.Recruit.Employer.Web.Models.AddLocation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations;

public class AddLocationEditModelValidator : AbstractValidator<AddLocationEditModel>
{
    public const string NotNullErrorMessage = "Enter a postcode to find the address or select ‘Enter address manually’";
    public const string MaxLengthErrorMessage = "Enter a postcode to find the address or select ‘Enter address manually’";
    public const string InvalidPostcodeErrorMessage = "Enter a recognised postcode or select ‘Enter address manually’";
    public const string MustBeEnglishPostcode = "Postcode must be in England. Your apprenticeship must be in England to advertise it on this service";
    
    public AddLocationEditModelValidator()
    {
        RuleFor(x => x.Postcode)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage(NotNullErrorMessage)
            .MaximumLength(8)
            .WithMessage(MaxLengthErrorMessage)
            .ValidPostCode()
            .WithMessage(InvalidPostcodeErrorMessage);
    }
}