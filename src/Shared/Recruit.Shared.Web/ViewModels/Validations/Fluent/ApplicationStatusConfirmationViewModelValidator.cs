using Esfa.Recruit.Shared.Web.ViewModels.ApplicationReview;
using Esfa.Recruit.Vacancies.Client.Application.Validation.Fluent;
using FluentValidation;

namespace Esfa.Recruit.Shared.Web.ViewModels.Validations.Fluent
{
    public class ApplicationStatusConfirmationViewModelValidator : AbstractValidator<IApplicationStatusConfirmationEditViewModel>
    {
        public ApplicationStatusConfirmationViewModelValidator()
        {
            RuleFor(x => x.NotifyApplicant)
                .NotNull()
                .WithMessage(ApplicationReviewValidator.NotifyApplicantRequired);           
        }
    }
}
