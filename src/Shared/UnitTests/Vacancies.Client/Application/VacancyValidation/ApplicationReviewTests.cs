﻿using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Vacancies.Client.Application.VacancyValidation
{
    public class ApplicationReviewTests
    {
        public class ShouldRequireCandiateFeedbackIfUnsuccessfulTestData : TheoryData<string, string>
        {
            public ShouldRequireCandiateFeedbackIfUnsuccessfulTestData()
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
        [ClassData(typeof(ShouldRequireCandiateFeedbackIfUnsuccessfulTestData))]
        public void ShouldRequireCandiateFeedbackIfUnsuccessful(string candidateFeedback, string expectedErrorMessage)
        {
            var m = new Recruit.Vacancies.Client.Domain.Entities.ApplicationReview
            {
                Status = ApplicationReviewStatus.Unsuccessful,
                CandidateFeedback = candidateFeedback
            };

            var validator = new ApplicationReviewValidator();

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
        [InlineData(ApplicationReviewStatus.Successful, null)]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback")]
        public void ApplicationReviewShouldBeValid(ApplicationReviewStatus status, string feedback)
        {
            var m = new Recruit.Vacancies.Client.Domain.Entities.ApplicationReview
            {
                Status = status,
                CandidateFeedback = feedback
            };

            var validator = new ApplicationReviewValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeTrue();
        }
    }
}
