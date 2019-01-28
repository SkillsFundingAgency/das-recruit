using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Application.VacancyValidation
{
    public class VacancyReviewTests
    {
        public class ManualQaCommentTestData : TheoryData<string, string>
        {
            public ManualQaCommentTestData()
            {
                var manualQaCommentTooLong =
                    new string('a',VacancyReviewValidator.ManualQaCommentMaxLength + 1);

                Add(">", VacancyReviewValidator.ManualQaCommentFreeTextCharactersMessage);
                Add(manualQaCommentTooLong, string.Format(VacancyReviewValidator.ManualQaCommentLengthMessage, VacancyReviewValidator.ManualQaCommentMaxLength));
            }
        }

        [Theory]
        [ClassData(typeof(ManualQaCommentTestData))]
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

        [Fact]
        public void ShouldNotRequireCandiateFeedbackIfSuccessful()
        {
            var m = new Recruit.Vacancies.Client.Domain.Entities.ApplicationReview
            {
                Status = ApplicationReviewStatus.Successful,
                CandidateFeedback = "should not specify feedback if successful"
            };

            var validator = new ApplicationReviewValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(ApplicationReviewValidator.CandidateFeedbackNull);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
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
}
