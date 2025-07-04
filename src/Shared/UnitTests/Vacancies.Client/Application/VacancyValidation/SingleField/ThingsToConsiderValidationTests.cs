using Esfa.Recruit.UnitTests.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class OtherRequirementsValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("considerations")]
        public void NoErrorsWhenOtherRequirementsIsValid(string text)
        {
            var vacancy = new Vacancy
            {
                OtherRequirements = text
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OtherRequirements);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void OtherRequirementsMustBe4000CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                OtherRequirements = "name".PadRight(4001, 'w')
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OtherRequirements);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OtherRequirements));
            result.Errors[0].ErrorCode.Should().Be("75");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OtherRequirements);
        }

        [Theory]
        [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
        public void OtherRequirementsMustNotContainInvalidCharacters(string invalidChar)
        {
            var vacancy = new Vacancy
            {
                OtherRequirements = invalidChar
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OtherRequirements);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OtherRequirements));
            result.Errors[0].ErrorCode.Should().Be("76");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.OtherRequirements);
        }

        [Theory]
        [InlineData("some text bother")]
        [InlineData("some text dang")]
        [InlineData("some text drat")]
        [InlineData("some text balderdash")]
        public void OtherRequirements_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                OtherRequirements = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OtherRequirements);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.OtherRequirements));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("613");
        }

        [Theory]
        [InlineData("some textbother")]
        [InlineData("some textdang")]
        [InlineData("some textdrat")]
        [InlineData("some textbalderdash")]
        public void OtherRequirements_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                OtherRequirements = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.OtherRequirements);
            result.HasErrors.Should().BeFalse();
        }
    }
}