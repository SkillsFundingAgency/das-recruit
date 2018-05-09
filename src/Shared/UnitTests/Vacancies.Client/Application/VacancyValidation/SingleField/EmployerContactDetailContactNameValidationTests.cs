using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public partial class EmployerContactDetailValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("contact name")]
        public void NoErrorsWhenEmployerContactNameIsValid(string contactName)
        {
            var vacancy = new Vacancy
            {
                EmployerContactName = contactName
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void EmployerContactNameMustBe100CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                EmployerContactName = "name".PadRight(101, 'w')
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactName));
            result.Errors[0].ErrorCode.Should().Be("90");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }

        [Theory]
        [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
        public void EmployerContactNameMustNotContainsInvalidCharacters(string invalidChar)
        {
            var vacancy = new Vacancy
            {
                EmployerContactName = invalidChar
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactName));
            result.Errors[0].ErrorCode.Should().Be("91");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }
    }
}