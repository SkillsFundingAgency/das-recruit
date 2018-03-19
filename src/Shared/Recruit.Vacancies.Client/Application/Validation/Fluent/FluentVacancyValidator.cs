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

        private void ValidateOrganisation()
        {
            When(x => x.Location != null, () => 
            {
                RuleFor(x => x.Location.AddressLine1)
                    .NotEmpty().WithMessage("{PropertyName} is a required field").WithErrorCode("123")
                    .MaximumLength(5)
                    .RunCondition(VacancyValidations.Organisation);
            });
        }

        private void ValidateDescription()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("{PropertyName} is a required field").WithErrorCode("123")
                .MaximumLength(100)
                .RunCondition(VacancyValidations.Title);
        }
    }
}