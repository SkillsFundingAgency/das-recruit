using System.Collections.Generic;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class ClosingDateValidationTests : VacancyValidationTestsBase
{
    public static IEnumerable<object[]> InvalidClosingDates =>
        new List<object[]>
        {
            new object[] { DateTime.UtcNow.Date },
            new object[] { DateTime.UtcNow },
            //new object[] { DateTime.UtcNow.AddDays(13) }
        };

    private class StubTimeProvider : ITimeProvider
    {
        public DateTime Now { get; set; }
        public DateTime OneHour => Now.AddHours(1);
        public DateTime Today => Now.Date;
        public DateTime NextDay => Now.Date.AddDays(1);
        public DateTime NextDay6am => Now.Date.AddDays(1).AddHours(6);
    }

    private const string ErrorCode_ClosingDateTooSoon = "18";

    private static void AssertInvalidForClosingDate(
        EntityValidationResult result,
        string expectedErrorCode = null)
    {
        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);

        var error = result.Errors[0];
        error.PropertyName.Should().Be(nameof(Vacancy.ClosingDate));
        error.RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);

        if (!string.IsNullOrEmpty(expectedErrorCode))
        {
            error.ErrorCode.Should().Be(expectedErrorCode);
        }
    }

    private static void AssertValid(EntityValidationResult result)
    {
        result.HasErrors.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void NoErrorsWhenClosingDateIsValid()
    {
        var vacancy = new Vacancy
        {
            ClosingDate = DateTime.UtcNow.AddDays(15)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

        result.HasErrors.Should().BeFalse();
        result.Errors.Should().HaveCount(0);
    }

    [Fact]
    public void ClosingDateMustHaveAValue()
    {
        var vacancy = new Vacancy
        {
            ClosingDate = null
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

        result.HasErrors.Should().BeTrue();
        result.Errors.Should().HaveCount(1);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
        result.Errors[0].ErrorCode.Should().Be("16");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
    }

    [Theory]
    [MemberData(nameof(InvalidClosingDates))]
    public void ClosingDateMustBeGreaterThanToday(DateTime closingDateValue)
    {
        var vacancy = new Vacancy
        {
            ClosingDate = closingDateValue
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

        AssertInvalidForClosingDate(result, expectedErrorCode: ErrorCode_ClosingDateTooSoon);
        result.Errors[0].PropertyName.Should().Be($"{nameof(vacancy.ClosingDate)}");
        result.Errors[0].RuleId.Should().Be((long)VacancyRuleSet.ClosingDate);
    }

    [Fact]
    public void NewAdvert_ClosingDateLessThan7DaysFromToday_IsInvalid()
    {
        var stubTime = new StubTimeProvider { Now = new DateTime(2025, 01, 01) };
        TimeProvider = stubTime;

        var vacancy = new Vacancy
        {
            Status = VacancyStatus.Draft,
            ClosingDate = stubTime.Now.Date.AddDays(6)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

        AssertInvalidForClosingDate(result, expectedErrorCode: ErrorCode_ClosingDateTooSoon);
    }

    [Fact]
    public void NewAdvert_ClosingDateExactly7DaysFromToday_IsValid()
    {
        var stubTime = new StubTimeProvider { Now = new DateTime(2025, 01, 01) };
        TimeProvider = stubTime;

        var vacancy = new Vacancy
        {
            Status = VacancyStatus.Draft,
            ClosingDate = stubTime.Now.Date.AddDays(7)
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);

        AssertValid(result);
    }

    [Fact]
    public void LiveExtension_ClosingDateCannotBeInPast_AllowsToday()
    {
        var stubTime = new StubTimeProvider { Now = new DateTime(2025, 01, 10) };
        TimeProvider = stubTime;

        var vacancy = new Vacancy
        {
            Status = VacancyStatus.Live,
            ClosingDate = stubTime.Now.Date.AddDays(-1) // yesterday
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);
        Assert.NotNull(result);
        Assert.NotEmpty(result.Errors);

        vacancy.ClosingDate = stubTime.Now.Date; // today
        var resultToday = Validator.Validate(vacancy, VacancyRuleSet.ClosingDate);
        Assert.NotNull(resultToday);
        Assert.Empty(resultToday.Errors);
    }
}