using System.Collections.Generic;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewEditModelTests
    {
        private Mock<IProfanityListProvider> _mockProfanityListProvider;

        [SetUp]
        public void Setup()
        {
            _mockProfanityListProvider = new Mock<IProfanityListProvider>();
            _mockProfanityListProvider.Setup(x => x.GetProfanityListAsync()).Returns(GetProfanityListAsync());
        }


        [Test]
        public void ShouldRequireOutcome()
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = null
            };

            var validator = new ApplicationReviewEditModelValidator(_mockProfanityListProvider.Object);

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(ApplicationReviewValidator.OutcomeRequired);
        }

        private static IEnumerable<object[]> _testCases = [
            [null, ApplicationReviewValidator.CandidateFeedbackRequired], 
            ["", ApplicationReviewValidator.CandidateFeedbackRequired],
            [">", ApplicationReviewValidator.CandidateFeedbackFreeTextCharacters],
            [null, ApplicationReviewValidator.CandidateFeedbackRequired],
            [new string('a', ApplicationReviewValidator.CandidateFeedbackMaxLength + 1), string.Format(ApplicationReviewValidator.CandidateFeedbackLength, ApplicationReviewValidator.CandidateFeedbackMaxLength)] 
        ];

        [TestCaseSource(nameof(_testCases))]
        public void ShouldRequireCandidateFeedbackIfUnsuccessful(string candidateFeedback, string expectedErrorMessage)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = ApplicationReviewStatus.Unsuccessful,
                CandidateFeedback = candidateFeedback
            };

            var validator = new ApplicationReviewEditModelValidator(_mockProfanityListProvider.Object);

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [TestCase(ApplicationReviewStatus.Successful, null)]
        [TestCase(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback")]        
        public void ShouldBeValid(ApplicationReviewStatus outcome, string feedback)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = outcome,
                CandidateFeedback = feedback
            };

            var validator = new ApplicationReviewEditModelValidator(_mockProfanityListProvider.Object);

            var result = validator.Validate(m);

            result.IsValid.Should().BeTrue();
        }
        
        [Test]
        public void ShouldBeInvalid()
        {
            var m = new ApplicationReviewEditModel {
                Outcome = ApplicationReviewStatus.Unsuccessful,
                CandidateFeedback = "?$@#()\"\'\\!,+-=_:;.&€£*%/[] \\A-Z \a-z \0-9 your comments will be sent to the candidate."
            };

            var validator = new ApplicationReviewEditModelValidator(_mockProfanityListProvider.Object);

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
        }

        [TestCase(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback bother")]
        [TestCase(ApplicationReviewStatus.Unsuccessful, "dang Some candidate feedback")]
        [TestCase(ApplicationReviewStatus.Unsuccessful, "Some candidate balderdash bother")]
        [TestCase(ApplicationReviewStatus.Unsuccessful, "Some drat feedback bother")]
        public void ShouldBeInvalid_ForProfanityWordsInFeedback(ApplicationReviewStatus outcome, string feedback)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = outcome,
                CandidateFeedback = feedback
            };

            var validator = new ApplicationReviewEditModelValidator(_mockProfanityListProvider.Object);
            var result = validator.Validate(m);
            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorCode.Should().Be("617");
        }
        public Task<IEnumerable<string>> GetProfanityListAsync()
        {
            return Task.FromResult<IEnumerable<string>>(new[] { "bother", "dang", "balderdash", "drat" });
        }
    }

    
}
