using Esfa.Recruit.Vacancies.Client.Application.Configuration;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class DurationValidationTests : VacancyValidationTestsBase
    {
        [Theory]
        [InlineData(DurationUnit.Month, 12, "30")]
        [InlineData(DurationUnit.Year, 1,   "30")]
        [InlineData(DurationUnit.Week, 52,  "30")]
        [InlineData(DurationUnit.Month, 13, "30")]
        [InlineData(DurationUnit.Month, 12)]
        public void NoErrorsWhenDurationFieldsAreValid(DurationUnit unitValue, int durationValue, string weeklyHoursText = null)
        {
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

        [Fact]
        public void DurationUnitMustHaveAValue()
        {
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

        [Fact]
        public void DurationUnitMustHaveAValidValue()
        {
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

        [Fact]
        public void DurationMustHaveAValue()
        {
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
        [InlineData(DurationUnit.Month, 8)]
        public void ApprenticeshipDurationMustBeAtLeast8Months(DurationUnit unitValue, int durationValue)
        {
            Feature.Setup(x=>x.IsFeatureEnabled("FoundationApprenticeships")).Returns(true);
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
        [InlineData(DurationUnit.Month, 8, "29", true)]
        [InlineData(DurationUnit.Month, 8, "30", false)]
        [InlineData(DurationUnit.Year, 1, "29", true)]
        [InlineData(DurationUnit.Month, 8, "19", true)]
        [InlineData(DurationUnit.Month, 9, "19", true)]
        [InlineData(DurationUnit.Month, 11, "19", true)]
        [InlineData(DurationUnit.Month, 15, "24", false)]
        [InlineData(DurationUnit.Year, 2, "14", true)]
        [InlineData(DurationUnit.Year, 2, "29", false)]
        [InlineData(DurationUnit.Year, 1, "9", true)]
        public void AnyApprenticeshipDurationMonthsMustHave30WeeklyHours(DurationUnit unitValue, int durationValue, string weeklyHoursText, bool hasErrors)
        {
            Feature.Setup(x=>x.IsFeatureEnabled("FoundationApprenticeships")).Returns(true);
            decimal? weeklyHours = decimal.TryParse(weeklyHoursText, out decimal parsed) ? parsed : (decimal?)null;
            var vacancy = new Vacancy
            {
                StartDate = new DateTime(2025,08,01),
                Wage = new Wage
                {
                    WeeklyHours = weeklyHours,
                    DurationUnit = unitValue,
                    Duration = durationValue
                }
            };
            int expectedNumberOfMonths = (int) Math.Ceiling(30 / vacancy.Wage.WeeklyHours.GetValueOrDefault() * 8);

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
    }
}