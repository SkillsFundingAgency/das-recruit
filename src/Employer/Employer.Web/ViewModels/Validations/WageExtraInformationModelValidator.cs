using Esfa.Recruit.Employer.Web.ViewModels.Part1.Wage;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class WageExtraInformationModelValidator : AbstractValidator<WageExtraInformationViewModel>
    {
        public const int WageAdditionalInformationMaxLength = 250;
        public const string WageAdditionalInformationLength = "Character count exceeded";
        public const string WageAdditionalInformationFreeTextCharacters = "You have entered invalid characters";
        public WageExtraInformationModelValidator()
        {
            RuleFor(x => x.WageAdditionalInformation);
            When(x => !string.IsNullOrEmpty(x.WageAdditionalInformation), () =>
            {
                RuleFor(x => x.WageAdditionalInformation)
                    .MaximumLength(WageAdditionalInformationMaxLength)
                    .WithMessage(string.Format(WageAdditionalInformationLength))
                    .ValidFreeTextCharacters()
                    .WithMessage(WageAdditionalInformationFreeTextCharacters);
            });
        }
    }
}