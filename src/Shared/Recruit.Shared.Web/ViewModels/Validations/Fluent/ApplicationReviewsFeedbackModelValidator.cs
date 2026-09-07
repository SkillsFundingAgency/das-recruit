using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReviews;
using Esfa.Recruit.Vacancies.Client.Application.Providers;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent
{
    public class ApplicationReviewsFeedbackModelValidator : AbstractValidator<IApplicationReviewsEditModel>
    {
        public ApplicationReviewsFeedbackModelValidator(
            IProfanityListProvider profanityListProvider) =>
            RuleFor(x => x.CandidateFeedback)
                .NotEmpty()
                .WithMessage(x => x.IsMultipleApplications
                    ? ApplicationReviewValidator.CandidateFeedbackRequiredForMultipleApplications
                    : ApplicationReviewValidator.CandidateFeedbackRequiredForSingleApplication)
                .ApplyCandidateFeedbackRules(profanityListProvider);
    }

    public class ApplicationReviewFeedbackModelValidator : AbstractValidator<IApplicationReviewEditModel>
    {
        public ApplicationReviewFeedbackModelValidator(
            IProfanityListProvider profanityListProvider) =>
            RuleFor(x => x.CandidateFeedback)
                .NotEmpty()
                .WithMessage(ApplicationReviewValidator.CandidateFeedbackRequiredForSingleApplication)
                .ApplyCandidateFeedbackRules(profanityListProvider);
        }
    
    public static class ApplicationReviewValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> ApplyCandidateFeedbackRules<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            IProfanityListProvider profanityListProvider) =>
            ruleBuilder
                .MaximumLength(ApplicationReviewValidator.CandidateFeedbackMaxLength)
                .WithMessage(string.Format(
                    ApplicationReviewValidator.CandidateFeedbackLength,
                    ApplicationReviewValidator.CandidateFeedbackMaxLength))
                .Must(ApplicationReviewValidator.BeWithinMaxWordsOrEmpty)
                .WithMessage(string.Format(
                    ApplicationReviewValidator.CandidateFeedbackWordsLength,
                    ApplicationReviewValidator.CandidateFeedbackMaxWordLength))
                .ValidFreeTextCharacters()
                .WithMessage(ApplicationReviewValidator.CandidateFeedbackFreeTextCharacters)
                .ProfanityCheck(profanityListProvider)
                .WithMessage(ApplicationReviewValidator.CandidateFeedbackProfanityPhrases)
                .WithErrorCode("617");
    }
}