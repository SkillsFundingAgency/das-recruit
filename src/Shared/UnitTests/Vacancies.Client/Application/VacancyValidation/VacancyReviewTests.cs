using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using System.Collections.Generic;
using NUnit.Framework;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation;

public class VacancyReviewTests
{
    private static IEnumerable<TestCaseData> ManualQaCommentTestData()
    {
        var manualQaCommentTooLong =
            new string('a', VacancyReviewValidator.ManualQaCommentMaxLength + 1);

        yield return new TestCaseData(">", VacancyReviewValidator.ManualQaCommentFreeTextCharactersMessage);
        yield return new TestCaseData(manualQaCommentTooLong, string.Format(VacancyReviewValidator.ManualQaCommentLengthMessage, VacancyReviewValidator.ManualQaCommentMaxLength));
    }

    [Test]
    [TestCaseSource(nameof(ManualQaCommentTestData))]
    public void ShouldValidateManaulQaComment(string manualQaComment, string expectedErrorMessage)
    {
        var m = new VacancyReview
        {
            ManualQaComment = manualQaComment
        };

        var validator = new VacancyReviewValidator();

        var result = validator.Validate(m);

        result.IsValid.Should().BeFalse();
        result.Errors.Count.Should().Be(1);
        result.Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
    }

    [Test]
    public void ShouldNotRequireCandidateFeedbackIfSuccessful()
    {
        var m = new ApplicationReview
        {
            Status = ApplicationReviewStatus.Successful,
            CandidateFeedback = "should not specify feedback if successful"
        };

        var validator = new ApplicationReviewValidator();

        var result = validator.Validate(m);

        result.IsValid.Should().BeTrue();
        result.Errors.Count.Should().Be(0);
    }

    [Test]
    [TestCase(null)]
    [TestCase("")]
    public void ApplicationReviewShouldBeValid(string manualQaComment)
    {
        var m = new VacancyReview
        {
            ManualQaComment = manualQaComment
        };

        var validator = new VacancyReviewValidator();

        var result = validator.Validate(m);

        result.IsValid.Should().BeTrue();
    }
}
