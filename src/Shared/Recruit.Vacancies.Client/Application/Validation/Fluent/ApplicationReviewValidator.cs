using System;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent
{
    public class ApplicationReviewValidator : AbstractValidator<ApplicationReview>
    {
        public const int CandidateFeedbackMaxLength = 5000;
        public const int CandidateFeedbackMaxWordLength = 200;

        public const string OutcomeRequired = "You must select one option before continuing";
        public const string CandidateFeedbackRequired = "You must say why the application was unsuccessful";
        public const string CandidateFeedbackLength = "Your feedback must be less than {0} characters";
        public const string CandidateFeedbackWordsLength = "Your feedback must be less than {0} words";
        public const string CandidateFeedbackFreeTextCharacters = "You have entered invalid characters";
        public const string CandidateFeedbackNull = "You must not provide feedback for a successful application";
        public const string NotifyCandidateRequired = "You must select one option";
        public const string CandidateFeedbackProfanityPhrases = "Feedback must not contain a banned word or phrase.";

        public ApplicationReviewValidator()
        {
            When(x => x.Status == ApplicationReviewStatus.Unsuccessful, () =>
            {
                RuleFor(x => x.CandidateFeedback)
                    .NotEmpty()
                    .WithMessage(CandidateFeedbackRequired)
                    .MaximumLength(CandidateFeedbackMaxLength)
                    .WithMessage(string.Format(CandidateFeedbackLength, CandidateFeedbackMaxLength))
                    .Must(BeWithinMaxWordsOrEmpty)
                    .WithMessage(string.Format(ApplicationReviewValidator.CandidateFeedbackWordsLength, ApplicationReviewValidator.CandidateFeedbackMaxWordLength))
                    .ValidFreeTextCharacters()
                    .WithMessage(CandidateFeedbackFreeTextCharacters);
            });

            When(x => x.Status == ApplicationReviewStatus.Successful, () =>
            {
                RuleFor(x => x.CandidateFeedback)
                    .Empty()
                    .WithMessage(CandidateFeedbackNull);
            });
        }

        public static bool BeWithinMaxWordsOrEmpty(string inputText)
        {
            if (string.IsNullOrEmpty(inputText))
            {
                return true;
            }

            string[] words = inputText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            return words.Length <= ApplicationReviewValidator.CandidateFeedbackMaxWordLength;
        }
    }
}
