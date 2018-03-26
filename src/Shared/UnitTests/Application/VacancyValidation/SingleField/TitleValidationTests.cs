using System;
using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using UnitTests.Application.VacancyValidation;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class ValidateVacancyTests : VacancyValidationTestsBase
    {
        public static IEnumerable<object[]> ValidTitles =>
            new List<object[]>
            {
                new object[] { new String('a', 100) },
                new object[] { new String('a', 1) }
            };

        [Theory]
        [MemberData(nameof(ValidTitles))]
        public void NoErrorsWhenTitleFieldIsValid(string validTitle)
        {
            var vacancy = new Vacancy 
            {
                Title = validTitle
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void TitleMustHaveAValue(string titleValue)
        {
            var vacancy = new Vacancy 
            {
                Title = titleValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("1");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Fact]
        public void TitleBeLongerThan100Characters()
        {
            var vacancy = new Vacancy 
            {
                Title = new String('a', 110)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("2");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void TitleMustContainVaildCharacters(string testValue)
        {
            var vacancy = new Vacancy 
            {
                Title = testValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Title);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Title));
            result.Errors[0].ErrorCode.Should().Be("3");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Title);
        }
    }
}