using System;
using System.Collections.Generic;
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
        public static IEnumerable<object[]> ValidTitles =>
            new List<object[]>
            {
                new object[] { new String('a', 100) },
                new object[] { new String('a', 1) }
            };

        [Theory]
        [MemberData(nameof(ValidTitles))]
        public void ShouldNotThrowExceptionIfTitleIsValid(string validTitle)
        {
            var validator = new VacancyValidator(new FluentVacancyValidator());

            var vacancy = new Vacancy 
            {
                Title = validTitle
            };

            Action act = () => validator.ValidateAndThrow(vacancy, VacancyRuleSet.Title);

            act.Should().NotThrow<EntityValidationException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TitleMustHaveAValue(string titleValue)
        {
            var validator = new VacancyValidator(new FluentVacancyValidator());

            var vacancy = new Vacancy 
            {
                Title = titleValue
            };

            Action act = () => validator.ValidateAndThrow(vacancy, VacancyRuleSet.Title);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("1");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Fact]
        public void TitleBeLongerThan100Characters()
        {
            var validator = new VacancyValidator(new FluentVacancyValidator());

            var vacancy = new Vacancy 
            {
                Title = new String('a', 110)
            };

            Action act = () => validator.ValidateAndThrow(vacancy, VacancyRuleSet.Title);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("2");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void TitleMustContainVaildCharacters(string testValue)
        {
            var validator = new VacancyValidator(new FluentVacancyValidator());

            var vacancy = new Vacancy 
            {
                Title = testValue
            };

            Action act = () => validator.ValidateAndThrow(vacancy, VacancyRuleSet.Title);

            var ex = act.Should().Throw<EntityValidationException>();
            ex.Which.ValidationResult.HasErrors.Should().BeTrue();
            ex.Which.ValidationResult.Errors.Count.Should().Be(1);
            ex.Which.ValidationResult.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            ex.Which.ValidationResult.Errors[0].ErrorCode.Should().Be("3");
            ex.Which.ValidationResult.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }
    }
}