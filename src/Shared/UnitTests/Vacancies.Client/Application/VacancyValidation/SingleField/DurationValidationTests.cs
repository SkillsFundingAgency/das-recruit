using System;
using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class DurationValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(DurationUnit.Month, 12,  "Apprenticeship", "30")]
        [InlineData(DurationUnit.Year, 1,  "Apprenticeship", "30")]
        [InlineData(DurationUnit.Week, 52,  "Apprenticeship", "30")]
        [InlineData(DurationUnit.Month, 13,  "Apprenticeship", "30")]
        [InlineData(DurationUnit.Month, 12,  "Apprenticeship")]
        [InlineData(DurationUnit.Week, 6,  "Traineeship")]
        [InlineData(DurationUnit.Week, 52, "Traineeship")]
        [InlineData(DurationUnit.Month, 12, "Traineeship")]
        [InlineData(DurationUnit.Month, 2, "Traineeship")]
        public void NoErrorsWhenDurationFieldsAreValid(DurationUnit unitValue, int durationValue, string serviceType, string weeklyHoursText = null)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            decimal? weeklyHours = decimal.TryParse(weeklyHoursText, out decimal parsed) ? parsed : (decimal?)null;
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WeeklyHours = weeklyHours,
                    DurationUnit = unitValue,
                    Duration = durationValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeFalse();
            result.Errors.Should().HaveCount(0);
        }

        [Theory]
        [InlineData("Apprenticeship")]
        [InlineData("Traineeship")]
        public void DurationUnitMustHaveAValue(string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = null,
                    Duration = 13
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.DurationUnit)}");
            result.Errors[0].ErrorCode.Should().Be("34");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }

        [Theory]
        [InlineData("Apprenticeship")]
        [InlineData("Traineeship")]
        public void DurationUnitMustHaveAValidValue(string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = (DurationUnit)1000,
                    Duration = 13
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.DurationUnit)}");
            result.Errors[0].ErrorCode.Should().Be("34");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }

        [Theory]
        [InlineData("Apprenticeship")]
        [InlineData("Traineeship")]
        public void DurationMustHaveAValue(string serviceType)
        {
            ServiceParameters = new ServiceParameters(serviceType);
            var vacancy = new Vacancy 
            {
                Wage = new Wage
                {
                    DurationUnit = DurationUnit.Month,
                    Duration = null
                }
            };
            
            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.Duration)}");
            result.Errors[0].ErrorCode.Should().Be("34");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }

        [Theory]
        [InlineData(DurationUnit.Month, 11)]
        public void ApprenticeshipDurationMustBeAtLeast12Months(DurationUnit unitValue, int durationValue)
        {
            ServiceParameters = new ServiceParameters("Apprenticeship");
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = unitValue,
                    Duration = durationValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.Duration)}");
            result.Errors[0].ErrorCode.Should().Be("36");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }
        
        [Theory]
        [InlineData(DurationUnit.Month, 12, "29", true)]
        [InlineData(DurationUnit.Month, 12, "30", false)]
        [InlineData(DurationUnit.Year, 1, "29", true)]
        [InlineData(DurationUnit.Month, 12, "19", true)]
        [InlineData(DurationUnit.Month, 13, "19", true)]
        [InlineData(DurationUnit.Month, 15, "19", true)]
        [InlineData(DurationUnit.Month, 15, "24", false)]
        [InlineData(DurationUnit.Year, 2, "14", true)]
        [InlineData(DurationUnit.Year, 2, "29", false)]
        [InlineData(DurationUnit.Year, 1, "9", true)]
        public void AnyApprenticeshipDurationMonthsMustHave30WeeklyHours(DurationUnit unitValue, int durationValue, string weeklyHoursText, bool hasErrors)
        {
            ServiceParameters = new ServiceParameters("Apprenticeship");
            decimal? weeklyHours = decimal.TryParse(weeklyHoursText, out decimal parsed) ? parsed : (decimal?)null;
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    WeeklyHours = weeklyHours,
                    DurationUnit = unitValue,
                    Duration = durationValue
                }
            };
            int expectedNumberOfMonths = (int) Math.Ceiling(30 / vacancy.Wage.WeeklyHours.GetValueOrDefault() * 12);

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            if (!hasErrors)
            {
                result.HasErrors.Should().BeFalse();
            }
            else
            {
                result.HasErrors.Should().BeTrue();
                result.Errors.Should().HaveCount(1);
                result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.Duration)}");
                result.Errors[0].ErrorCode.Should().Be("36");
                result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
                result.Errors[0].ErrorMessage.Should()
                    .Be($"Duration of apprenticeship must be {expectedNumberOfMonths} months based on the number of hours per week entered");
            }
            
        }

        [Theory]
        [InlineData(DurationUnit.Month, 1)]
        [InlineData(DurationUnit.Week, 5)]
        [InlineData(DurationUnit.Month, 13)]
        [InlineData(DurationUnit.Week, 53)]
        public void TraineeshipDurationMustBeAtLeast6WeeksAndNoLongerThan12Months(DurationUnit unitValue, int durationValue)
        {
            ServiceParameters = new ServiceParameters("Traineeship");
            var vacancy = new Vacancy
            {
                Wage = new Wage
                {
                    DurationUnit = unitValue,
                    Duration = durationValue
                }
            };

            var result = Validator.Validate(vacancy, VacancyRuleSet.Duration);

            result.HasErrors.Should().BeTrue();
            result.Errors.Should().HaveCount(1);
            result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.Wage)}.{nameof(vacancy.Wage.Duration)}");
            result.Errors[0].ErrorCode.Should().Be("36");
            result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.Duration);
        }
    }
}