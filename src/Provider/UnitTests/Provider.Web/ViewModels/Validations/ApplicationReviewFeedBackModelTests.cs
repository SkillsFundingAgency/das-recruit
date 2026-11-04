using System.Linq;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using System.Collections.Generic;
using NUnit.Framework;
using Esfa.Recruit.Provider.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Provider.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Moq;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation;

public class ApplicationReviewFeedbackModelTests
{
    public static IEnumerable<TestCaseData> ShouldRequireCandiateFeedbackIfUnsuccessfulTestData()
    {
        var candidateFeedbackTooLong =
            new string('a', ApplicationReviewValidator.CandidateFeedbackMaxLength + 1);

        yield return new TestCaseData(null, ApplicationReviewValidator.CandidateFeedbackRequired);
        yield return new TestCaseData("", ApplicationReviewValidator.CandidateFeedbackRequired);
        yield return new TestCaseData(">", ApplicationReviewValidator.CandidateFeedbackFreeTextCharacters);
        yield return new TestCaseData(candidateFeedbackTooLong, string.Format(ApplicationReviewValidator.CandidateFeedbackLength, ApplicationReviewValidator.CandidateFeedbackMaxLength));
    }

    [Test]
    [TestCaseSource(nameof(ShouldRequireCandiateFeedbackIfUnsuccessfulTestData))]
    public void ShouldRequireCandiateFeedbackIfUnsuccessful(string candidateFeedback, string expectedErrorMessage)
    {
        var profanityListProvider = Mock.Of<IProfanityListProvider>();

        var m = new ApplicationReviewFeedbackViewModel
        {
            Outcome = ApplicationReviewStatus.Unsuccessful,
            CandidateFeedback = candidateFeedback
        };

        var validator = new ApplicationReviewFeedbackModelValidator(profanityListProvider);

        var result = validator.Validate(m);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
    }

    [Test]
    public void CandidateFeedback_WithinMaxWords_ShouldPassValidation()
    {
        var profanityListProvider = Mock.Of<IProfanityListProvider>();
        var m = new ApplicationReviewFeedbackViewModel
        {
            Outcome = ApplicationReviewStatus.Unsuccessful,
            CandidateFeedback = "This is a sample feedback within the word limit."
        };

        var validator = new ApplicationReviewFeedbackModelValidator(profanityListProvider);

        var result = validator.Validate(m);

        result.IsValid.Should().BeTrue();
    }

    [Test]
    public void CandidateFeedback_ExceedsMaxWords_ShouldFailValidation()
    {
        var profanityListProvider = Mock.Of<IProfanityListProvider>();
        var m = new ApplicationReviewFeedbackViewModel
        {
            Outcome = ApplicationReviewStatus.Unsuccessful,
            CandidateFeedback = string.Join(" ", Enumerable.Repeat("word", ApplicationReviewValidator.CandidateFeedbackMaxWordLength + 1))
        };

        var validator = new ApplicationReviewFeedbackModelValidator(profanityListProvider);

        var result = validator.Validate(m);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be(string.Format(ApplicationReviewValidator.CandidateFeedbackWordsLength, ApplicationReviewValidator.CandidateFeedbackMaxWordLength));
    }

    [Test]
    public void CandidateFeedback_ExceedsMaxCharacters_ShouldFailValidation()
    {
        var profanityListProvider = Mock.Of<IProfanityListProvider>();
        var m = new ApplicationReviewFeedbackViewModel
        {
            Outcome = ApplicationReviewStatus.Unsuccessful,
            CandidateFeedback = new string('W', ApplicationReviewValidator.CandidateFeedbackMaxLength + 1)
        };

        var validator = new ApplicationReviewFeedbackModelValidator(profanityListProvider);

        var result = validator.Validate(m);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be(string.Format(ApplicationReviewValidator.CandidateFeedbackLength, ApplicationReviewValidator.CandidateFeedbackMaxLength));
    }

    [Test]
    [TestCase(ApplicationReviewStatus.Successful, null)]
    [TestCase(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback")]
    public void ApplicationReviewShouldBeValid(ApplicationReviewStatus status, string feedback)
    {
        var profanityListProvider = Mock.Of<IProfanityListProvider>();
        var m = new ApplicationReviewFeedbackViewModel
        {
            Outcome = status,
            CandidateFeedback = feedback
        };

        var validator = new ApplicationReviewFeedbackModelValidator(profanityListProvider);

        var result = validator.Validate(m);

        result.IsValid.Should().BeTrue();
    }
}
