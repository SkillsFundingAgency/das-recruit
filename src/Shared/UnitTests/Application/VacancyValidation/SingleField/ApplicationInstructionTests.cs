using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public class ApplicationInstructionTests : VacancyValidationTestsBase
    {       

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("You can apply online or through post.")]
        public void NoErrorsWhenOfflineApplicationUrlIsValid(string instructions)
        {
            var vacancy = new Vacancy
            {
                ApplicationInstructions = instructions
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationInstructions);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void OfflineApplicationUrlMustBe500CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                ApplicationInstructions = "instructions".PadRight(501, 'w')
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationInstructions);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationInstructions));
            result.Errors[0].ErrorCode.Should().Be("88");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationInstructions);
        }

        [Theory]
        [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
        public void OfflineApplicationUrlMustNotContainsInvalidCharacters(string invalidChar)
        {
            var vacancy = new Vacancy
            {
                ApplicationInstructions = invalidChar
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.ApplicationInstructions);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.ApplicationInstructions));
            result.Errors[0].ErrorCode.Should().Be("89");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ApplicationInstructions);
        }
    }
}