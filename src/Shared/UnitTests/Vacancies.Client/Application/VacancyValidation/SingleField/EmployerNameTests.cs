using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class EmployerNameTests : VacancyValidationTestsBase
{
    [Test]
    public void RegisteredName_ShouldValidateEmpty()
    {
        var vacancy = new Vacancy() {
            EmployerName = string.Empty
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerName);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
        result.Errors[0].ErrorCode.Should().Be("4");
    }

    [Test]
    public void TradingName_ShouldValidateEmpty()
    {
        var vacancy = new Vacancy() {
            EmployerName = string.Empty,
            EmployerNameOption = EmployerNameOption.TradingName
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
        result.Errors[0].ErrorCode.Should().Be("401");
    }

    [Test]
    public void TradingName_ShouldValidateSpecialCharactersAndLength()
    {
        var vacancy = new Vacancy() {
            EmployerName = "£$$%$%£$<>" + new string('a', 100),
            EmployerNameOption = EmployerNameOption.TradingName
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.TradingName);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(2);
        result.Errors.Any(e => e.ErrorCode == "402").Should().BeTrue();
        result.Errors.Any(e => e.ErrorCode == "403").Should().BeTrue();
    }

    [Test]
    public void Anonymous_ShouldValidateEmpty()        
    {
        var vacancy = new Vacancy()
        {
            EmployerName = string.Empty,
            EmployerNameOption = EmployerNameOption.Anonymous,
            AnonymousReason = "a valid reason"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.EmployerName));
        result.Errors[0].ErrorCode.Should().Be("405");
    }

    [Test]
    public void Anonymous_ShouldValidateSpecialCharactersAndLength()
    {
        var vacancy = new Vacancy() {
            EmployerName = "£$$%$%£$<>" + new string('a', 100),
            EmployerNameOption = EmployerNameOption.Anonymous,
            AnonymousReason = "a valid reason"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(2);
        result.Errors.Any(e => e.ErrorCode == "406").Should().BeTrue();
        result.Errors.Any(e => e.ErrorCode == "407").Should().BeTrue();
    }

    [Test]
    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
    public void Anonymous_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            EmployerName = freeText,
            EmployerNameOption = EmployerNameOption.Anonymous,
            AnonymousReason = "a valid reason"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("603");
    }

    [Test]
    [TestCase("some textbother")]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
    public void Anonymous_Should_Not_FailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            EmployerName = freeText,
            EmployerNameOption = EmployerNameOption.Anonymous,
            AnonymousReason = "a valid reason"
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
        result.HasErrors.Should().BeFalse();
    }
}
