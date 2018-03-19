using System;
using Esfa.Recruit.Vacancies.Client.Application.Exceptions;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.Validation
{
    public class ValidateVacancyTests
    {
        [Fact]
        public void TitleCannotBeEmpty()
        {
            var validator = new VacancyValidator(new FluentVacancyValidator());

            var vacancy = new Vacancy 
            {
                Title = ""
            };

            Action act = () => validator.ValidateAndThrow(vacancy, VacancyValidations.Title);

            // TODO: LWA Need a nicer way of running these assertions to get better messages on failure. Try/catch or Custom assertion extension method?
            act.Should().Throw<EntityValidationException>()
                .Where(e => e.ValidationResult.HasErrors == true)
                .Where(e => e.ValidationResult.Errors.Count == 1)
                .Where(e => e.ValidationResult.Errors[0].PropertyName == nameof(vacancy.Title))
                .Where(e => e.ValidationResult.Errors[0].ErrorMessage == "Title is a required field")
                .Where(e => e.ValidationResult.Errors[0].ErrorCode == "123");
        }
    }
}