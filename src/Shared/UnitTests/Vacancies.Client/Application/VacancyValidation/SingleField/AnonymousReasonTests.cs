using Esfa.Recruit.Vacancies.Client.Application.Validation;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using NUnit.Framework;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation.SingleField;

public class AnonymousReasonTests : VacancyValidationTestsBase
{
    [Test]
    public void EmployerWeb_Anonymous_ShouldValidateEmpty()        
    {
        var vacancy = new Vacancy()
        {
            EmployerName = "a valid anonymous name",
            EmployerNameOption = EmployerNameOption.Anonymous,
            SourceOrigin = SourceOrigin.EmployerWeb,
            AnonymousReason = null
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
        result.Errors[0].ErrorCode.Should().Be("408");
    }

    [Test]
    public void EmployerWeb_Anonymous_ShouldValidateSpecialCharactersAndLength()
    {
        var vacancy = new Vacancy() {
            EmployerName = "a valid anonymous name",
            EmployerNameOption = EmployerNameOption.Anonymous,
            SourceOrigin = SourceOrigin.EmployerWeb,
            AnonymousReason = "�$$%$%�$<>" + new string('a', 200),
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);

        result.HasErrors.Should().BeTrue();
        result.Errors.Count.Should().Be(2);
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
        result.Errors[0].ErrorCode.Should().Be("409");
        result.Errors[1].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
        result.Errors[1].ErrorCode.Should().Be("410");
    }

    [Test]
    [TestCase("some text bother")]
    [TestCase("some text dang")]
    [TestCase("some text drat")]
    [TestCase("some text balderdash")]
    public void EmployerWeb_Anonymous_ShouldFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            EmployerName = "a valid anonymous name",
            EmployerNameOption = EmployerNameOption.Anonymous,
            SourceOrigin = SourceOrigin.EmployerWeb,
            AnonymousReason = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
        result.HasErrors.Should().BeTrue();
        result.Errors[0].PropertyName.Should().Be(nameof(vacancy.AnonymousReason));
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorCode.Should().Be("604");
    }

    [Test]
    [TestCase("some textdang")]
    [TestCase("some textdrat")]
    [TestCase("some textbalderdash")]
    public void EmployerWeb_Anonymous_ShouldNotFailIfContainsWordsFromTheProfanityList(string freeText)
    {
        var vacancy = new Vacancy()
        {
            EmployerName = "a valid anonymous name",
            EmployerNameOption = EmployerNameOption.Anonymous,
            SourceOrigin = SourceOrigin.EmployerWeb,
            AnonymousReason = freeText
        };

        var result = Validator.Validate(vacancy, VacancyRuleSet.EmployerNameOption);
        result.HasErrors.Should().BeFalse();
    }
}
