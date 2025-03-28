using Esfa.Recruit.UnitTests.Vacancies.Client.Application;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
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
                EmployerDescription = new String('a', 4001)
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

        [Theory]
        [InlineData("some text bother")]
        [InlineData("some text dang")]
        [InlineData("some text drat")]
        [InlineData("some text balderdash")]
        public void EmployerDescription_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                Description = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);
            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.Description));
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("609");
        }

        [Theory]
        [InlineData("some textbother")]
        [InlineData("some textdang")]
        [InlineData("some textdrat")]
        [InlineData("some textbalderdash")]
        public void EmployerDescription_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
        {
            var vacancy = new Vacancy()
            {
                Description = freeText
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Description);
            result.HasErrors.Should().BeFalse();
        }
    }
}