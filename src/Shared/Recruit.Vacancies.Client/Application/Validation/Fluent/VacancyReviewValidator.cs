using Esfa.Recruit.Vacancies.Client.Domain.Entities;
using FluentValidation;

namespace Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;

public class VacancyReviewValidator : AbstractValidator<VacancyReview>
{
    public const int ManualQaCommentMaxLength = 2500;

    public const string ManualQaCommentLengthMessage = "Your comment must be less than {0} characters";
    public const string ManualQaCommentFreeTextCharactersMessage = "You have entered invalid characters";
    public const string ManualQaCommentRequired = "If you are rejecting this vacancy you must include comments";

    public VacancyReviewValidator()
    {
        RuleFor(x => x.ManualQaComment)
            .MaximumLength(ManualQaCommentMaxLength)
            .WithMessage(string.Format(ManualQaCommentLengthMessage, ManualQaCommentMaxLength))
            .ValidFreeTextCharacters()
            .WithMessage(ManualQaCommentFreeTextCharactersMessage);

        When(x => x.ManualOutcome == ManualQaOutcome.Referred, () =>
        {
            RuleFor(x => x.ManualQaComment)
                .NotEmpty()
                .WithMessage(ManualQaCommentRequired);
        });
    }
}