using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Esfa.Recruit.Vacancies.Client.Infrastructure.Slack;
using Moq;
using Xunit;

namespace Esfa.Recruit.Vacancies.Client.UnitTests.Infrastructure.Slack
{
    public class SlackNotifyVacancyReviewUpdatesTests
    {
        [Fact]
        public async Task WhenPostingVacancyReadyForFirstReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview{VacancyReference = vacancyReference, SubmissionCount = 1};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("ready for review") && 
                        !s.Text.Contains(" (1st submission)") && 
                        s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<Emojis>(e => e.Equals(Emojis.New))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewCreated(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyReadyForReReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview{VacancyReference = vacancyReference, SubmissionCount = 2};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s =>
                        s.Text.Contains("ready for review") && 
                        s.Text.Contains(" (2nd submission)") && 
                        s.Text.Contains(vacancyReference.ToString())
                    ), 
                    It.Is<Emojis>(e => e.Equals(Emojis.New))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewCreated(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyApprovedFromFirstReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview{VacancyReference = vacancyReference, SubmissionCount = 1};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("has been approved") && 
                        !s.Text.Contains(" (1st review)") && 
                        s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<Emojis>(e => e.Equals(Emojis.Approved))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewApproved(vacancyReview);

            mockSlackClient.Verify();
        }

        [Fact]
        public async Task WhenPostingVacancyApprovedFromReReviewMessage_ThenItShouldPostAMessageWithTheNewIconAndSuitableMessage()
        {
            var vacancyReference = 1000000001;
            var vacancyReview = new VacancyReview {VacancyReference = vacancyReference, SubmissionCount = 2};
            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s =>
                        s.Text.Contains("has been approved") &&
                        s.Text.Contains(" (2nd review)") &&
                        s.Text.Contains(vacancyReference.ToString())),
                    It.Is<Emojis>(e => e.Equals(Emojis.Approved))))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

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
                AutomatedQaOutcomeIndicators = automatedQaOutcomeIndicators
            };

            var mockSlackClient = new Mock<ISlackClient>();

            mockSlackClient
                .Setup(c => c.Post(
                    It.Is<SlackMessage>(s => 
                        s.Text.Contains("has been referred") && 
                        s.Text.Contains(" (2 issues)") && 
                        s.Text.Contains(vacancyReference.ToString())), 
                    It.Is<Emojis>(e => e.Equals(Emojis.Referred))))
                    .Returns(Task.CompletedTask)
                    .Verifiable();

            var client = new SlackNotifyVacancyReviewUpdates(mockSlackClient.Object);

            await client.VacancyReviewReferred(vacancyReview);

            mockSlackClient.Verify();
        }
    }
}