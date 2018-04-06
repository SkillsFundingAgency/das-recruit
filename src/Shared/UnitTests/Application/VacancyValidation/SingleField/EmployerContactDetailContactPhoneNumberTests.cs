﻿using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Application.VacancyValidation.SingleField
{
    public partial class EmployerContactDetailTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("02086695847")]
        public void NoErrorsWhenEmployerContactPhoneIsValid(string phoneNo)
        {
            var vacancy = new Vacancy
            {
                EmployerContactPhone = phoneNo
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void EmployerContactPhoneNumberMustBe16CharactersOrLess()
        {
            var vacancy = new Vacancy
            {
                EmployerContactPhone = "+4402086695847".PadRight(101, 'w')
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactPhone));
            result.Errors[0].ErrorCode.Should().Be("95");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }

        [Fact]
        public void EmployerContactPhoneNumberMustBe8CharactersOrMore()
        {
            var vacancy = new Vacancy
            {
                EmployerContactPhone = "0208"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactPhone));
            result.Errors[0].ErrorCode.Should().Be("96");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }

        [Fact]
        public void EmployerContactPhoneNumberMustBeValidEmailFormat()
        {
            var vacancy = new Vacancy
            {
                EmployerContactPhone = "543121***"
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerContactDetails);

            result.HasErrors.Should().BeTrue();
            result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerContactPhone));
            result.Errors[0].ErrorCode.Should().Be("97");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.EmployerContactDetails);
        }
    }
}