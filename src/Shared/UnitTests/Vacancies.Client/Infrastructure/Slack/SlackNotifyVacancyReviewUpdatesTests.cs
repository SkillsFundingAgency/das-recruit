using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Esfa.Recruit.UnitTests.Vacancies.Client.Infrastructure.Slack
{
    public class SlackNotifyVacancyReviewUpdatesTests
    {
        [Fact]
        public async Task WhenPostingVacancyReadyForFirstReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview{VacancyReference = vacancyReference, SubmissionCount = 1, VacancySnapshot = new Vacancy { OwnerType = OwnerType.Employer }};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("ready for review") && 
                        !s.Text.Contains(" (1st submission)") && 
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.New))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.VacancyReviewCreated(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyReadyForReReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview{VacancyReference = vacancyReference, SubmissionCount = 2, VacancySnapshot = new Vacancy { OwnerType = OwnerType.Employer }};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s =>
                        s.Text.Contains("ready for review") && 
                        s.Text.Contains(" (2nd submission)") && 
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())
                    ), 
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.New))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.VacancyReviewCreated(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyApprovedFromFirstReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview{VacancyReference = vacancyReference, SubmissionCount = 1, VacancySnapshot = new Vacancy { OwnerType = OwnerType.Employer }};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("has been approved") && 
                        !s.Text.Contains(" (1st review)") && 
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.Approved))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.VacancyReviewApproved(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyApprovedFromReReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview {VacancyReference = vacancyReference, SubmissionCount = 2, VacancySnapshot = new Vacancy { OwnerType = OwnerType.Employer }};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s =>
                        s.Text.Contains("has been approved") &&
                        s.Text.Contains(" (2nd review)") &&
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())),
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.Approved))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.VacancyReviewApproved(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyReferredMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;

            var manualQaFieldIndicators = new List<ManualQaFieldIndicator>
            {
                new ManualQaFieldIndicator {FieldIdentifier = "field1", IsChangeRequested = true},
                new ManualQaFieldIndicator {FieldIdentifier = "field2", IsChangeRequested = false}
            };

            var automatedQaOutcomeIndicators = new List<RuleOutcomeIndicator>
            {
                new RuleOutcomeIndicator {RuleOutcomeId = Guid.NewGuid(), IsReferred = true},
                new RuleOutcomeIndicator {RuleOutcomeId = Guid.NewGuid(), IsReferred = false}
            };

            var vacancyReview = new VacancyReview
            {
                VacancyReference = vacancyReference, 
                ManualQaFieldIndicators = manualQaFieldIndicators, 
                AutomatedQaOutcomeIndicators = automatedQaOutcomeIndicators,
                VacancySnapshot = new Vacancy { OwnerType = OwnerType.Employer }
            };

            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.PostAsync(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("has been referred") && 
                        s.Text.Contains(" (2 issues)") && 
                        s.Text.Contains(" (Employer)") && 
                        s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<SlackVacancyNotificationType>(e => e.Equals(SlackVacancyNotificationType.Referred))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object, Options.Create(new SlackConfiguration()));

            await client.VacancyReviewReferred(vacancyReview);

            mockSlackClient.Verify();
        }
    }
}