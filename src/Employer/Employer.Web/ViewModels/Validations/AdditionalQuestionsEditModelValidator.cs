using Esfa.Recruit.Employer.Web.ViewModels.Part2.AdditionalQuestions;
using FluentValidation;

namespace Esfa.Recruit.Employer.Web.ViewModels.Validations
{
    public class AdditionalQuestionsEditModelValidator : AbstractValidator<AdditionalQuestionsEditModel>
    {
        public AdditionalQuestionsEditModelValidator(int questionCount)
        {
            RuleFor(x => x.AdditionalQuestion1)
                .Cascade(CascadeMode.Continue)
                .MaximumLength(250)
                    .WithMessage($"Question {questionCount - 1} must not exceed 250 characters.")
                    .WithErrorCode("AQ1_Length")
                .Must(q => string.IsNullOrWhiteSpace(q) || q.Contains("?"))
                    .WithMessage($"Question {questionCount - 1} must include a question mark (‘?’).")
                    .WithErrorCode("AQ1_QuestionMark");

            RuleFor(x => x.AdditionalQuestion2)
                .Cascade(CascadeMode.Continue)
                .MaximumLength(250)
                    .WithMessage($"Question {questionCount} must not exceed 250 characters.")
                    .WithErrorCode("AQ2_Length")
                .Must(q => string.IsNullOrWhiteSpace(q) || q.Contains("?"))
                    .WithMessage($"Question {questionCount} must include a question mark (‘?’).")
                    .WithErrorCode("AQ2_QuestionMark");
        }
    }
}
