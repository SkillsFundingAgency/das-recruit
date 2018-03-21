using System;
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
                .RunCondition(VacancyValidations.Title);
        }

        private void ValidateOrganisation()
        {
            RuleFor(x => x.OrganisationId)
                .NotEmpty()
                    .WithMessage("You must select one organisation")
                    .WithErrorCode("4")
                .RunCondition(VacancyValidations.OrganisationId);


            RuleFor(x => x.Location)
                .SetValidator(new AddressValidator())
                .RunCondition(VacancyValidations.OrganisationAddress);
        }
    }
}