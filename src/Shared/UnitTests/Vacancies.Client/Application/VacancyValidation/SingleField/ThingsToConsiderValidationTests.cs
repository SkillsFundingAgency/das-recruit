using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class ThingsToConsiderValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("considerations")]
        public void NoErrorsWhenThingsToConsiderIsValid(string text)
        {
            var vacancy = new Vacancy
            {
                ThingsToConsider = text
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ThingsToConsider);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void ThingsToConsiderMustBe4000CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                ThingsToConsider = "name".PadRight(4001, 'w')
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ThingsToConsider);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ThingsToConsider));
            result.Errors[0].ErrorCode.Should().Be("75");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ThingsToConsider);
        }

        [Theory]
        [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
        public void ThingsToConsiderMustNotContainInvalidCharacters(string invalidChar)
        {
            var vacancy = new Vacancy
            {
                ThingsToConsider = invalidChar
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ThingsToConsider);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ThingsToConsider));
            result.Errors[0].ErrorCode.Should().Be("76");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ThingsToConsider);
        }

        [Theory]
        [InlineData("some text bother")]
        [InlineData("some text dang")]
        [InlineData("some text drat")]
        [InlineData("some text balderdash")]
        public void ThingsToConsider_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                ThingsToConsider = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ThingsToConsider);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ThingsToConsider));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("613");
        }

        [Theory]
        [InlineData("some textbother")]
        [InlineData("some textdang")]
        [InlineData("some textdrat")]
        [InlineData("some textbalderdash")]
        public void ThingsToConsider_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                ThingsToConsider = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ThingsToConsider);
            result.HasErrors.Should().BeFalse();
        }
    }
}