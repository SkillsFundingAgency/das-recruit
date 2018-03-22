using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public class FluentVacancyValidator : AbstractValidator<Vacancy>
    {
        public FluentVacancyValidator()
        {
            ValidateDescription();

            ValidateOrganisation();

            ValidateNumberOfPositions();

            ValidateShortDescription();
        }

        private void ValidateDescription()
        {
            RuleFor(x => x.Title)
                .NotEmpty()
                    .WithMessage("Enter the title of the vacancy")
                    .WithErrorCode("1")
                .MaximumLength(100)
                    .WithMessage("The title must not be more than {MaxLength}")
                    .WithErrorCode("2")
                .ValidFreeTextCharacters()
                    .WithMessage("The title contains some invalid characters")
                    .WithErrorCode("3")
                .RunCondition(VacancyRuleSet.Title);
        }

        private void ValidateOrganisation()
        {
            RuleFor(x => x.OrganisationId)
                .NotEmpty()
                    .WithMessage("You must select one organisation")
                    .WithErrorCode("4")
                .RunCondition(VacancyRuleSet.OrganisationId);


            RuleFor(x => x.Location)
                .SetValidator(new AddressValidator())
                .RunCondition(VacancyRuleSet.OrganisationAddress);
        }

        private void ValidateNumberOfPositions()
        {
            RuleFor(x => x.NumberOfPositions)
                .Must(x => x.HasValue && x.Value > 0)
                    .WithMessage("Enter the number of positions for this vacancy")
                    .WithErrorCode("10")
                .RunCondition(VacancyRuleSet.NumberOfPostions);
        }

        private void ValidateShortDescription()
        {
            RuleFor(x => x.ShortDescription)
                .Cascade(CascadeMode.StopOnFirstFailure)
                .NotEmpty()
                .WithMessage("Enter the brief overview of the vacancy")
                    .WithErrorCode("12")
                .MaximumLength(350)
                    .WithMessage("The overview of the vacancy must not be more than 350 characters")
                    .WithErrorCode("13")
                .MinimumLength(50)
                    .WithMessage("The overview of the vacancy must be more than 50 characters")
                    .WithErrorCode("14")
                .ValidFreeTextCharacters()
                    .WithMessage("The overview of the vacancy contains some invalid characters")
                    .WithErrorCode("15")
                .RunCondition(VacancyRuleSet.ShortDescription);
        }
    }
}