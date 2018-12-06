using System;
using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class SlackNotifyVacancyReviewUpdates : INotifyVacancyReviewUpdates
    {
        private readonly ISlackClient _slackClient;

        public SlackNotifyVacancyReviewUpdates(ISlackClient slackClient)
        {
            _slackClient = slackClient;
        }

        public Task VacancyReviewCreated(VacancyReview vacancyReview)
        {
            var messageBody = $"Vacancy VAC{vacancyReview.VacancyReference} is ready for review";

            if (vacancyReview.SubmissionCount > 1) messageBody += $" ({GetNumericSuffix(vacancyReview.SubmissionCount)} submission)";

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.Post(message, Emojis.New);
        }

        public Task VacancyReviewReferred(VacancyReview vacancyReview)
        {
            var messageBody = $"Vacancy VAC{vacancyReview.VacancyReference} has been referred";

            var issueCount = vacancyReview.ManualQaFieldIndicators.Count(i => i.IsChangeRequested) +
                             vacancyReview.AutomatedQaOutcomeIndicators.Count(i => i.IsReferred);

            if (issueCount == 1) messageBody += $" ({issueCount} issue)";
            if (issueCount > 1) messageBody += $" ({issueCount} issues)";

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.Post(message, Emojis.Referred);
        }

        public Task VacancyReviewApproved(VacancyReview vacancyReview)
        {
            var messageBody = $"Vacancy VAC{vacancyReview.VacancyReference} has been approved";

            if (vacancyReview.SubmissionCount > 1) messageBody += $" ({GetNumericSuffix(vacancyReview.SubmissionCount)} review)";

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.Post(message, Emojis.Approved);
        }

        private static string GetNumericSuffix(int count)
        {
            if (count > 10 && count < 14)
                return $"{count}th";

            var lastDigit = count.ToString().ToCharArray().Last();

            switch (lastDigit)
            {
                case '1':
                    return $"{count}st";
                case '2':
                    return $"{count}nd";
                case '3':
                    return $"{count}rd";
                default:
                    return $"{count}th";
            }
        }
    }
}
