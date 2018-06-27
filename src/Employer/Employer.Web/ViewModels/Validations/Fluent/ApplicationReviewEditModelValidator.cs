using Esfa.Recruit.Employer.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations.Fluent
{
    public class ApplicationReviewEditModelValidator : AbstractValidator<ApplicationReviewEditModel>
    {
        public ApplicationReviewEditModelValidator()
        {
            RuleFor(x => x.Outcome)
                .NotNull()
                .WithMessage(ApplicationReviewValidator.OutcomeRequired);

            When(x => x.Outcome == ApplicationReviewStatus.Unsuccessful, () =>
            {
                RuleFor(x => x.CandidateFeedback)
                    .NotEmpty()
                    .WithMessage(ApplicationReviewValidator.CandidateFeedbackRequired)
                    .MaximumLength(ApplicationReviewValidator.CandidateFeedbackMaxLength)
                    .WithMessage(string.Format(ApplicationReviewValidator.CandidateFeedbackLength, ApplicationReviewValidator.CandidateFeedbackMaxLength))
                    .ValidFreeTextCharacters()
                    .WithMessage(ApplicationReviewValidator.CandidateFeedbackFreeTextCharacters);
            });
        }
    }
}
