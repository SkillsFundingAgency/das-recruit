﻿using System;
using System.Collections.Generic;
using System.Text;
using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace Esfa.Recruit.Employer.UnitTests.Employer.Web.ViewModels.ApplicationReview
{
    public class ApplicationReviewEditModelTests
    {
        [Fact]
        public void ShouldRequireOutcome()
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = null
            };

            var validator = new ApplicationReviewEditModelValidator();

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

            var validator = new ApplicationReviewEditModelValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeFalse();
            result.Errors.Count.Should().Be(1);
            result.Errors[0].ErrorMessage.Should().Be(expectedErrorMessage);
        }

        [Theory]
        [InlineData(ApplicationReviewStatus.Successful, null)]
        [InlineData(ApplicationReviewStatus.Unsuccessful, "Some candidate feedback")]
        public void ShouldVeValid(ApplicationReviewStatus outcome, string feedback)
        {
            var m = new ApplicationReviewEditModel
            {
                Outcome = outcome,
                CandidateFeedback = feedback
            };

            var validator = new ApplicationReviewEditModelValidator();

            var result = validator.Validate(m);

            result.IsValid.Should().BeTrue();
        }
    }
}
