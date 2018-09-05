using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Qa.Web.ViewModels.Validations
{
    public class ReviewEditModelValidator : AbstractValidator<ReviewEditModel>
    {
        public ReviewEditModelValidator()
        {
            RuleFor(x => x.ReviewerComment)
                .MaximumLength(VacancyReviewValidator.ManualQaCommentMaxLength)
                .WithMessage(string.Format(VacancyReviewValidator.ManualQaCommentLengthMessage, VacancyReviewValidator.ManualQaCommentMaxLength))
                .ValidFreeTextCharacters()
                .WithMessage(VacancyReviewValidator.ManualQaCommentFreeTextCharactersMessage);
        }
    }
}
