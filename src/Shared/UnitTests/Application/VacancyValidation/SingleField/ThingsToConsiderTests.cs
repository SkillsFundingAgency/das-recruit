using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public partial class ThingsToConsiderTests : VacancyValidationTestsBase
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
        public void ThingsToConsiderMustBe350CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                ThingsToConsider = "name".PadRight(351, 'w')
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
    }
}