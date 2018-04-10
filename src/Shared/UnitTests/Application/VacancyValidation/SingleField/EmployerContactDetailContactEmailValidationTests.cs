﻿using Esfa.Recruit.Vacancies.Client.Application.Validation;
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
        [InlineData("joebloggs@company.com")]
        public void NoErrorsWhenEmployerContactEmailIsValid(string emailAddress)
        {
            var vacancy = new Vacancy
            {
                EmployerContactEmail = emailAddress
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void EmployerContactEmailMustBe100CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                EmployerContactEmail = "name@".PadRight(101, 'w')
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactEmail));
            result.Errors[0].ErrorCode.Should().Be("92");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }

        [Theory]
        [MemberData(nameof(TestData.BlacklistedCharacters), MemberType = typeof(TestData))]
        public void EmployerContactEmailMustNotContainsInvalidCharacters(string invalidChar)
        {
            var vacancy = new Vacancy
            {
                EmployerContactEmail = invalidChar
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactEmail));
            result.Errors[0].ErrorCode.Should().Be("93");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }

        [Fact]
        public void EmployerContactEmailMustBeValidEmailFormat()
        {
            var vacancy = new Vacancy
            {
                EmployerContactEmail = "joe"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactEmail));
            result.Errors[0].ErrorCode.Should().Be("94");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }
    }
}