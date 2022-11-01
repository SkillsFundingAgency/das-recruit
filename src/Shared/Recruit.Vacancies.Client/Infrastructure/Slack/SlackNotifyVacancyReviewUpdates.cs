using System.Linq;
using System.Threading.Tasks;
using Esfa.Recruit.Vacancies.Client.Application.Services;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Esfa.Recruit.Vacancies.Client.Infrastructure.Slack
{
    internal class SlackNotifyVacancyReviewUpdates : INotifyVacancyReviewUpdates
    {
        private readonly string _findAnApprenticeshipDetailPrefixUrl;
        private readonly ISlackClient _slackClient;

        public SlackNotifyVacancyReviewUpdates(ISlackClient slackClient, IOptions<SlackConfiguration> slackConfig)
        {
            _slackClient = slackClient;
            _findAnApprenticeshipDetailPrefixUrl = slackConfig.Value.FindAnApprenticeshipDetailPrefixUrl;
        }

        public Task VacancyReviewCreated(VacancyReview vacancyReview)
        {
            var messageBody = $"Vacancy VAC{vacancyReview.VacancyReference} is ready for review ({vacancyReview.VacancySnapshot.OwnerType.ToString()})";

            if (vacancyReview.SubmissionCount > 1) messageBody += $" ({GetNumericSuffix(vacancyReview.SubmissionCount)} submission)";

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.PostAsync(message, SlackVacancyNotificationType.New);
        }

        public Task VacancyReviewReferred(VacancyReview vacancyReview)
        {
            var messageBody = $"Vacancy VAC{vacancyReview.VacancyReference} has been referred ({vacancyReview.VacancySnapshot.OwnerType.ToString()})";

            var issueCount = vacancyReview.ManualQaFieldIndicators.Count(i => i.IsChangeRequested) +
                             vacancyReview.AutomatedQaOutcomeIndicators.Count(i => i.IsReferred);

            if (issueCount == 1) messageBody += $" ({issueCount} issue)";
            if (issueCount > 1) messageBody += $" ({issueCount} issues)";

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.PostAsync(message, SlackVacancyNotificationType.Referred);
        }

        public Task VacancyReviewApproved(VacancyReview vacancyReview)
        {
            var messageBody = string.Format("Vacancy <{0}{1}|VAC{1}> has been approved ({2})", _findAnApprenticeshipDetailPrefixUrl, vacancyReview.VacancyReference, vacancyReview.VacancySnapshot.OwnerType.ToString());

            if (vacancyReview.SubmissionCount > 1) messageBody += $" ({GetNumericSuffix(vacancyReview.SubmissionCount)} review)";

            var message = new SlackMessage {Text = messageBody};

            return _slackClient.PostAsync(message, SlackVacancyNotificationType.Approved);
        }

        private string GetNumericSuffix(int count)
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
