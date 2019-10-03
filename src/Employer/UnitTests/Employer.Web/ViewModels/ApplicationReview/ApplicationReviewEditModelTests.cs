using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewEditModelTests
    {
        protected readonly TestProfanityListProvider MockProfanityListProvider;

        public ApplicationReviewEditModelTests()
        {
            MockProfanityListProvider = new TestProfanityListProvider();
        }

        [Fact]
        public void ShouldRequireOutcome()
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = null
            };

            var validator = new ApplicationReviewEditModelValidator(MockProfanityListProvider);

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(ApplicationReviewValidator.OutcomeRequired);
        }

        public class ShouldRequireCandidateFeedbackIfUnsuccessfulTestData : TheoryData<string, string>
        {
            public ShouldRequireCandidateFeedbackIfUnsuccessfulTestData()
            {
                var candidateFeedbackTooLong =
                    new string('a', ApplicationReviewValidator.CandidateFeedbackMaxLength + 1);

                Add(null, ApplicationReviewValidator.CandidateFeedbackRequired);
                Add("", ApplicationReviewValidator.CandidateFeedbackRequired);
                Add(">", ApplicationReviewValidator.CandidateFeedbackFreeTextCharacters);
                Add(candidateFeedbackTooLong, string.Format(ApplicationReviewValidator.CandidateFeedbackLength, ApplicationReviewValidator.CandidateFeedbackMaxLength));
            }
        }

        [Theory]
        [ClassData(typeof(ShouldRequireCandidateFeedbackIfUnsuccessfulTestData))]
        public void ShouldRequireCandidateFeedbackIfUnsuccessful(string candidateFeedback, string expectedErrorMessage)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = ApplicationReviewStatus.Unsuccessful,
                CandidateFeedback = candidateFeedback
            };

            var validator = new ApplicationReviewEditModelValidator(MockProfanityListProvider);

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Theory]
        [InlineData(ApplicationReviewStatus.Successful, null)]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback")]        
        public void ShouldBeValid(ApplicationReviewStatus outcome, string feedback)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = outcome,
                CandidateFeedback = feedback
            };

            var validator = new ApplicationReviewEditModelValidator(MockProfanityListProvider);

            var result = validator.Validate(m);

            result.IsValid.Should().BeTrue();
        }

        
        [Fact]
        public void ShouldBeInvalid()
        {
            var m = new ApplicationReviewEditModel {
                Outcome = ApplicationReviewStatus.Unsuccessful,
                CandidateFeedback = "?$@#()\"\'\\!,+-=_:;.&€£*%/[] \\A-Z \a-z \0-9 your comments will be sent to the candidate."
            };

            var validator = new ApplicationReviewEditModelValidator(MockProfanityListProvider);

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback bother")]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "dang Some candidate feedback")]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "Some candidate balderdash bother")]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "Some drat feedback bother")]
        public void ShouldBeInvalid_ForProfanityWordsInFeedback(ApplicationReviewStatus outcome, string feedback)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = outcome,
                CandidateFeedback = feedback
            };

            var validator = new ApplicationReviewEditModelValidator(MockProfanityListProvider);
            var result = validator.Validate(m);
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("617");
        }
    }

    public class TestProfanityListProvider : IProfanityListProvider
    {
        public Task<IEnumerable<string>> GetProfanityListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "bother", "dang", "balderdash", "drat" });
        }
    }
}
