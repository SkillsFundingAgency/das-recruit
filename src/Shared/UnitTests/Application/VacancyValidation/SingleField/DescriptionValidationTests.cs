using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class DescriptionValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenDescriptionFieldIsValid()
        {
            var vacancy = new Vacancy 
            {
                Description = "a valid description"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void DescriptionMustNotBeEmpty(string description)
        {
            var vacancy = new Vacancy 
            {
                Description = description
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
            result.Errors[0].ErrorCode.Should().Be("53");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Description);
        }

        [Fact]
        public void DescriptionMustNotBeLongerThanMaxLength()
        {
            var vacancy = new Vacancy 
            {
                Description = new String('a', 501)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
            result.Errors[0].ErrorCode.Should().Be("7");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Description);
        }

        [Fact]
        public void DescriptionMustContainVaildCharacters()
        {
            var vacancy = new Vacancy 
            {
                Description = "<"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
            result.Errors[0].ErrorCode.Should().Be("6");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Description);
        }
    }
}