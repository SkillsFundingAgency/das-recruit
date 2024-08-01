using System;
using System.Text;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class PositionValidationTests : VacancyValidationTestsBase
    {
        [Fact]
        public void NoErrorsWhenPositionFieldsAreValid()
        {
            var vacancy = new Vacancy
            {
                NumberOfPositions = 2,
                ShortDescription = new string('a', 60)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.NumberOfPositions | VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        public void NumberOfPositionMustHaveAValue(int? numOfPositionsValue)
        {
            var vacancy = new Vacancy 
            {
                NumberOfPositions = numOfPositionsValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.NumberOfPositions);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.NumberOfPositions));
            result.Errors[0].ErrorCode.Should().Be("10");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.NumberOfPositions);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void ShortDescriptionMustHaveAValue(string shortDescriptionValue)
        {
            var vacancy = new Vacancy
            {
                ShortDescription = shortDescriptionValue
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("12");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Fact]
        public void NoErrorsWhenShortDescriptionAreValid()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 350)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void ShortDescriptionMustNotBeMoreThan350Characters()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 351)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("13");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Fact]
        public void ShortDescriptionMustNotBeLessThan50Characters()
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 49)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("14");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }


        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void ShortDescriptionMustContainValidCharacters(string invalidCharacter)
        {
            var vacancy = new Vacancy
            {
                ShortDescription = new String('a', 30) + invalidCharacter + new String('b', 30)
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors[0].ErrorCode.Should().Be("15");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ShortDescription);
        }

        [Theory]
        [InlineData("some text bother random text random text random text random text")]
        [InlineData("some text dang random text random text random text random text")]
        [InlineData("some text drat random text random text random text random text")]
        [InlineData("some text balderdash random text random text random text random text")]
        public void ShortDescription_Should_Fail_IfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                ShortDescription = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ShortDescription));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("605");
        }

        [Theory]
        [InlineData("some textbother random text random text random text random text")]
        [InlineData("some textdang random text random text random text random text")]
        [InlineData("some textdrat random text random text random text random text")]
        [InlineData("some textbalderdash random text random text random text random text")]
        public void ShortDescription_Should_Not_Fail_IfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                ShortDescription = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ShortDescription);
            result.HasErrors.Should().BeFalse();
        }
    }
}