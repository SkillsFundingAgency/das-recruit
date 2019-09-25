using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class EmployerDescriptionValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenEmployerDescriptionFieldIsValid()
        {
            var vacancy = new Vacancy 
            {
                EmployerDescription = "a valid description"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

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
                EmployerDescription = description
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerDescription));
            result.Errors[0].ErrorCode.Should().Be("80");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerDescription);
        }

        [Fact]
        public void EmployerDescriptionMustNotBeLongerThanMaxLength()
        {
            var vacancy = new Vacancy 
            {
                EmployerDescription = new String('a', 501)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerDescription));
            result.Errors[0].ErrorCode.Should().Be("77");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerDescription);
        }

        [Theory]
        [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
        public void EmployerDescriptionMustContainValidCharacters(string invalidChar)
        {
            var vacancy = new Vacancy 
            {
                EmployerDescription = invalidChar
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerDescription));
            result.Errors[0].ErrorCode.Should().Be("78");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerDescription);
        }

        [Fact]
        public void EmployerDescription_ShouldFailIfContainsWordsFromTheProfanityList()
        {
            var vacancy = new Vacancy()
            {
                Description = "a vacancy description dangleberry"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("5");
        }
    }
}