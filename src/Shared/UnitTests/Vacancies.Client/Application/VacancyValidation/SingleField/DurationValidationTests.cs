using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField
{
    public class DurationValidationTests : VacancyValidationTestsBase
    {
        [TestCase(DurationUnit.Month, 12, "30")]
        [TestCase(DurationUnit.Year, 1,   "30")]
        [TestCase(DurationUnit.Week, 52,  "30")]
        [TestCase(DurationUnit.Month, 13, "30")]
        [TestCase(DurationUnit.Month, 12)]
        public void NoErrorsWhenDurationFieldsAreValid(DurationUnit unitValue, int durationValue, string weeklyHoursText = null)
        {
            decimal? weeklyHours = decimal.TryParse(weeklyHoursText, out decimal parsed) ? parsed : null;
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

        [Test]
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

        [Test]
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

        [Test]
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

        [TestCase(DurationUnit.Month, 11)]
        public void ApprenticeshipDurationMustBeAtLeast12Months(DurationUnit unitValue, int durationValue)
        {
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
        
        [TestCase(DurationUnit.Month, 12, "29", true)]
        [TestCase(DurationUnit.Month, 12, "30", false)]
        [TestCase(DurationUnit.Year, 1, "29", true)]
        [TestCase(DurationUnit.Month, 12, "19", true)]
        [TestCase(DurationUnit.Month, 13, "19", true)]
        [TestCase(DurationUnit.Month, 15, "19", true)]
        [TestCase(DurationUnit.Month, 15, "24", false)]
        [TestCase(DurationUnit.Year, 2, "14", true)]
        [TestCase(DurationUnit.Year, 2, "29", false)]
        [TestCase(DurationUnit.Year, 1, "9", true)]
        public void AnyApprenticeshipDurationMonthsMustHave30WeeklyHours(DurationUnit unitValue, int durationValue, string weeklyHoursText, bool hasErrors)
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
    }
}