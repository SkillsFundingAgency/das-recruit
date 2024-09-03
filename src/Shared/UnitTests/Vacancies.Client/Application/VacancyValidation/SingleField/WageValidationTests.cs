using System;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class WageValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(WageType.FixedWage, 30000, null)]
        [InlineData(WageType.NationalMinimumWage, null, null)]
        [InlineData(WageType.NationalMinimumWageForApprentices, null, null)]
        public void NoErrorsWhenWageFieldsAreValid(WageType wageTypeValue, int? yearlyFixedWageAmountValue, string wageAdditionalInfoValue)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = wageTypeValue,
                    FixedWageYearlyAmount = Convert.ToDecimal(yearlyFixedWageAmountValue),
                    WageAdditionalInformation = wageAdditionalInfoValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Fact]
        public void WageMustNotBeNull()
        {
            var vacancy = new Vacancy
            {
                Wage = null
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}");
            result.Errors[0].ErrorCode.Should().Be("46");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }

        [Fact]
        public void WageTypeMustHaveAValue()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = null
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WageType)}");
            result.Errors[0].ErrorCode.Should().Be("46");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }

        [Fact]
        public void WageTypeMustHaveAValidValue()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = (WageType)1000
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WageType)}");
            result.Errors[0].ErrorCode.Should().Be("46");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WageAdditionalInfoIsOptional(string descriptionValue)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = WageType.NationalMinimumWage,
                    WageAdditionalInformation = descriptionValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void WageAdditionalInfoMustContainValidCharacters(string invalidCharacter)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = WageType.NationalMinimumWage,
                    WageAdditionalInformation = new String('a', 50) + invalidCharacter + new String('a', 50)
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WageAdditionalInformation)}");
            result.Errors[0].ErrorCode.Should().Be("45");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }

        [Fact]
        public void WageAdditionalInfoMustBeLessThan251Characters()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = WageType.NationalMinimumWage,
                    WageAdditionalInformation = new string('a', 252)
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.WageAdditionalInformation)}");
            result.Errors[0].ErrorCode.Should().Be("44");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WageCompanyBenefitsInfoIsOptional(string descriptionValue)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = WageType.NationalMinimumWage,
                    CompanyBenefitsInformation = descriptionValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("<")]
        [InlineData(">")]
        public void WageCompanyBenefitsInfoMustContainValidCharacters(string invalidCharacter)
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = WageType.NationalMinimumWage,
                    CompanyBenefitsInformation = new String('a', 50) + invalidCharacter + new String('a', 50)
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.CompanyBenefitsInformation)}");
            result.Errors[0].ErrorCode.Should().Be("45");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }

        [Fact]
        public void WageCompanyBenefitsInfoMustBeLessThan251Characters()
        {
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WageType = WageType.NationalMinimumWage,
                    CompanyBenefitsInformation = new string('a', 252)
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Wage);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.CompanyBenefitsInformation)}");
            result.Errors[0].ErrorCode.Should().Be("44");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Wage);
        }
    }
}